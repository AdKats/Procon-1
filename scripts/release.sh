#!/usr/bin/env bash
#
# PRoCon Release Script
#
# Usage:
#   ./scripts/release.sh alpha      # Auto-increment alpha (v2.0.0-alpha.1 → v2.0.0-alpha.2)
#   ./scripts/release.sh stable     # Release stable (v2.0.0)
#   ./scripts/release.sh v2.1.0     # Release specific version
#   ./scripts/release.sh --retag    # Re-tag current version (delete + recreate at HEAD)
#
set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

info()  { echo -e "${CYAN}[INFO]${NC} $*"; }
ok()    { echo -e "${GREEN}[OK]${NC} $*"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $*"; }
err()   { echo -e "${RED}[ERROR]${NC} $*" >&2; }
die()   { err "$@"; exit 1; }

# --- Pre-flight checks ---

check_prerequisites() {
    command -v git >/dev/null || die "git not found"
    command -v gh >/dev/null || die "gh (GitHub CLI) not found"
    command -v dotnet >/dev/null && DOTNET=dotnet || DOTNET="$HOME/.dotnet/dotnet"
    [ -x "$DOTNET" ] || die "dotnet not found (tried dotnet and ~/.dotnet/dotnet)"

    # Must be in repo root
    [ -f "src/PRoCon.sln" ] || die "Run this script from the repository root"

    # Must be on master
    BRANCH=$(git rev-parse --abbrev-ref HEAD)
    [ "$BRANCH" = "master" ] || die "Must be on master branch (currently on $BRANCH)"

    # Working tree must be clean
    if ! git diff --quiet || ! git diff --cached --quiet; then
        die "Working tree is dirty. Commit or stash changes first."
    fi

    # Must be up to date with remote
    git fetch origin master --tags --quiet
    LOCAL=$(git rev-parse HEAD)
    REMOTE=$(git rev-parse origin/master)
    if [ "$LOCAL" != "$REMOTE" ]; then
        die "Local master ($LOCAL) differs from origin/master ($REMOTE). Pull or push first."
    fi

    ok "Pre-flight checks passed"
}

# --- Version helpers ---

get_latest_alpha() {
    git tag -l 'v*-alpha.*' --sort=-v:refname | head -1
}

get_latest_stable() {
    # v2+ tags without a pre-release suffix (exclude v1.x legacy)
    git tag -l 'v2*' --sort=-v:refname | grep -v '-' | head -1
}

next_alpha() {
    local latest
    latest=$(get_latest_alpha)
    if [ -z "$latest" ]; then
        echo "v2.0.0-alpha.1"
        return
    fi
    # Extract the alpha number and increment
    local num
    num=$(echo "$latest" | grep -oP 'alpha\.\K[0-9]+')
    local base
    base=$(echo "$latest" | sed 's/-alpha\.[0-9]*//')
    echo "${base}-alpha.$((num + 1))"
}

next_stable() {
    local latest
    latest=$(get_latest_stable)
    if [ -z "$latest" ]; then
        echo "v2.0.0"
        return
    fi
    # Increment patch version
    local ver="${latest#v}"
    local major minor patch
    IFS='.' read -r major minor patch <<< "$ver"
    echo "v${major}.${minor}.$((patch + 1))"
}

# --- Build verification ---

verify_build() {
    info "Building solution to verify no errors..."
    if ! $DOTNET build src/PRoCon.UI/PRoCon.UI.csproj -c Release --verbosity quiet 2>&1 | tail -3; then
        die "Build failed. Fix errors before releasing."
    fi
    ok "Build succeeded"
}

# --- Tag and release ---

create_tag() {
    local tag="$1"

    # Check if tag already exists
    if git rev-parse "$tag" >/dev/null 2>&1; then
        die "Tag $tag already exists. Use --retag to recreate it at HEAD."
    fi

    info "Creating tag: $tag"
    git tag "$tag"
    git push origin "$tag"
    ok "Tag $tag pushed"
}

retag() {
    local tag
    tag=$(get_latest_alpha)
    if [ -z "$tag" ]; then
        tag=$(get_latest_stable)
    fi
    [ -z "$tag" ] && die "No existing tags to retag"

    warn "Re-tagging $tag at HEAD ($(git rev-parse --short HEAD))"
    echo -n "Confirm? [y/N] "
    read -r confirm
    [ "$confirm" = "y" ] || [ "$confirm" = "Y" ] || die "Aborted"

    # Delete old release if it exists
    gh release delete "$tag" --yes 2>/dev/null || true

    # Delete remote and local tag
    git push origin ":refs/tags/$tag" 2>/dev/null || true
    git tag -d "$tag" 2>/dev/null || true

    # Recreate
    git tag "$tag"
    git push origin "$tag"
    ok "Re-tagged $tag at $(git rev-parse --short HEAD)"
}

# --- Monitor workflow ---

monitor_workflow() {
    local tag="$1"
    info "Waiting for workflow to start..."
    sleep 5

    local run_id
    run_id=$(gh run list --workflow=build-and-release.yml --limit 1 --json databaseId --jq '.[0].databaseId')
    [ -n "$run_id" ] || die "Could not find workflow run"

    info "Monitoring workflow run: $run_id"
    echo "  https://github.com/$(gh repo view --json nameWithOwner -q .nameWithOwner)/actions/runs/$run_id"
    echo ""

    # Poll until complete
    while true; do
        local status conclusion
        status=$(gh run view "$run_id" --json status --jq '.status')
        if [ "$status" = "completed" ]; then
            conclusion=$(gh run view "$run_id" --json conclusion --jq '.conclusion')
            break
        fi

        # Show job status
        local jobs
        jobs=$(gh run view "$run_id" --json jobs --jq '[.jobs[] | "\(.name): \(.status) \(.conclusion // "")"] | join(", ")')
        printf "\r  %s" "$jobs"
        sleep 10
    done

    echo ""
    if [ "$conclusion" = "success" ]; then
        ok "Workflow completed successfully!"

        # Show release URL
        local release_url
        release_url=$(gh release view "$tag" --json url --jq '.url' 2>/dev/null || echo "")
        if [ -n "$release_url" ]; then
            echo ""
            ok "Release: $release_url"
            echo ""
            # Show assets
            gh release view "$tag" --json assets --jq '.assets[].name' | while read -r asset; do
                echo "  - $asset"
            done
        fi
    else
        err "Workflow failed (conclusion: $conclusion)"
        echo ""
        # Show which jobs failed
        gh run view "$run_id" --json jobs --jq '.jobs[] | select(.conclusion=="failure") | "  FAILED: \(.name)"'
        echo ""
        echo "  View logs: gh run view $run_id --log"
        exit 1
    fi
}

# --- Main ---

main() {
    local mode="${1:-}"

    [ -z "$mode" ] && {
        echo "Usage: $0 <alpha|stable|vX.Y.Z|--retag>"
        echo ""
        echo "  alpha     Auto-increment alpha version"
        echo "  stable    Auto-increment stable patch version"
        echo "  vX.Y.Z   Release a specific version"
        echo "  --retag   Delete and recreate the latest tag at HEAD"
        echo ""
        echo "Current tags:"
        echo "  Latest alpha:  $(get_latest_alpha || echo 'none')"
        echo "  Latest stable: $(get_latest_stable || echo 'none')"
        exit 0
    }

    check_prerequisites

    if [ "$mode" = "--retag" ]; then
        retag
        local tag
        tag=$(get_latest_alpha)
        [ -z "$tag" ] && tag=$(get_latest_stable)
        monitor_workflow "$tag"
        return
    fi

    verify_build

    local tag
    case "$mode" in
        alpha)
            tag=$(next_alpha)
            ;;
        stable)
            tag=$(next_stable)
            warn "This will create a STABLE release: $tag"
            echo -n "Confirm? [y/N] "
            read -r confirm
            [ "$confirm" = "y" ] || [ "$confirm" = "Y" ] || die "Aborted"
            ;;
        v*)
            tag="$mode"
            ;;
        *)
            die "Unknown mode: $mode (use alpha, stable, or vX.Y.Z)"
            ;;
    esac

    info "Releasing: $tag"
    if [[ "$tag" == *"-"* ]]; then
        info "Type: pre-release"
    else
        info "Type: stable release"
    fi
    echo ""

    create_tag "$tag"
    monitor_workflow "$tag"
}

main "$@"

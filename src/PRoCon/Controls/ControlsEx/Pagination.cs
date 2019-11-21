using PRoCon.Controls.Data;
using System;
using System.Windows.Forms;

namespace PRoCon.Controls.ControlsEx
{
    public partial class Pagination : UserControl
    {
        /// <summary>
        /// The total number of items to display on each page.
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// The current page index
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// The maximum number of pages.
        /// </summary>
        public int MaximumPage
        {
            get { return this.Source != null ? (int)Math.Ceiling((decimal)this.Source.Count / this.ItemsPerPage) : 1; }
        }

        /// <summary>
        /// Source of data 
        /// </summary>
        public ISource Source
        {
            get { return _source; }
            set
            {
                _source = value;

                if (this._source != null)
                {
                    this._source.Changed += SourceOnChanged;
                }
            }
        }
        private ISource _source;

        /// <summary>
        /// The current page has changed.
        /// </summary>
        public event Action<Object, EventArgs> Changed;

        public Pagination()
        {
            InitializeComponent();

            this.CurrentPage = 1;
            this.ItemsPerPage = 20;
            this.Calculate();
        }

        /// <summary>
        /// Disable all actions
        /// </summary>
        protected void DisableAllActions()
        {
            this.First.Enabled = false;
            this.Previous.Enabled = false;
            this.Next.Enabled = false;
            this.Last.Enabled = false;
        }

        /// <summary>
        /// Enable all allowed actions
        /// </summary>
        protected void EnableAllowedActions()
        {
            this.First.Enabled = this.CurrentPage != 1;
            this.Previous.Enabled = this.CurrentPage != 1;
            this.Next.Enabled = this.CurrentPage != this.MaximumPage;
            this.Last.Enabled = this.CurrentPage != this.MaximumPage;
        }

        protected void UpdateSource()
        {
            if (this.Source != null)
            {
                this.Source.Take = this.ItemsPerPage;
                this.Source.Skip = (this.CurrentPage - 1) * this.ItemsPerPage;
            }
        }

        /// <summary>
        /// Disables all actions, then enables the allowed actions.
        /// </summary>
        protected void Calculate()
        {
            if (this.CurrentPage > this.MaximumPage)
            {
                this.CurrentPage = this.MaximumPage == 0 ? 1 : this.MaximumPage;
            }

            this.DisableAllActions();
            this.EnableAllowedActions();
            this.UpdateSource();

            this.Page.Text = String.Format(@"{0} / {1}", this.CurrentPage, this.MaximumPage == 0 ? 1 : this.MaximumPage);

        }

        /// <summary>
        /// Fires off the current page change event
        /// </summary>
        protected void OnChange()
        {
            var handler = this.Changed;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        private void First_Click(object sender, EventArgs e)
        {
            this.CurrentPage = 1;
            this.Calculate();

            this.OnChange();
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            this.CurrentPage = this.CurrentPage > 1 ? this.CurrentPage - 1 : 1;
            this.Calculate();

            this.OnChange();
        }

        private void Next_Click(object sender, EventArgs e)
        {
            this.CurrentPage = this.CurrentPage < this.MaximumPage ? this.CurrentPage + 1 : this.MaximumPage;
            this.Calculate();

            this.OnChange();
        }

        private void Last_Click(object sender, EventArgs e)
        {
            this.CurrentPage = this.MaximumPage;
            this.Calculate();

            this.OnChange();
        }

        private void SourceOnChanged()
        {
            this.Calculate();

            this.OnChange();
        }
    }
}

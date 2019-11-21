using System.Collections.Generic;

namespace PRoCon.Core.Players
{
    public class PlayerListSettings
    {

        public delegate void IndexChangedHandler(int index);
        public event IndexChangedHandler SplitTypeChanged;

        public delegate void PercentageChangedHandler(float percentage);
        public event PercentageChangedHandler TwoSplitterPercentageChanged;
        public event PercentageChangedHandler FourSplitterPercentageChanged;

        // return String.Format("{0} {1} {2} {3}", this.chkPlayerListShowTeams.Checked, this.m_iSplitPlayerLists, this.spltTwoSplit.SplitterDistance, this.spltFourSplit.SplitterDistance);

        private int m_iSplitType;
        public int SplitType
        {
            get
            {
                return this.m_iSplitType;
            }
            set
            {
                if (value == 1 || value == 2 || value == 4)
                {
                    this.m_iSplitType = value;
                }
                else
                {
                    this.m_iSplitType = 1;
                }

                if (this.SplitTypeChanged != null)
                {
                    this.SplitTypeChanged(this.m_iSplitType);
                }
            }
        }

        private float m_flTwoSplitterPercentage;
        public float TwoSplitterPercentage
        {
            get
            {
                return this.m_flTwoSplitterPercentage;
            }
            set
            {
                if (value < 0.0F || value > 1.0F)
                {
                    this.m_flTwoSplitterPercentage = 0.5F;
                }
                else
                {
                    this.m_flTwoSplitterPercentage = value;
                }

                if (this.TwoSplitterPercentageChanged != null)
                {
                    this.TwoSplitterPercentageChanged(this.m_flTwoSplitterPercentage);
                }
            }
        }

        private float m_flFourSplitterPercentage;
        public float FourSplitterPercentage
        {
            get
            {
                return this.m_flFourSplitterPercentage;
            }
            set
            {
                if (value < 0.0F || value > 1.0F)
                {
                    this.m_flFourSplitterPercentage = 0.5F;
                }
                else
                {
                    this.m_flFourSplitterPercentage = value;
                }

                if (this.FourSplitterPercentageChanged != null)
                {
                    this.FourSplitterPercentageChanged(this.m_flFourSplitterPercentage);
                }
            }
        }

        public List<string> Settings
        {
            get
            {
                return new List<string>() { "true", this.SplitType.ToString(), this.TwoSplitterPercentage.ToString(), this.FourSplitterPercentage.ToString() };
            }
            set
            {
                if (value.Count > 0)
                {
                    int iIndex = 0;
                    float flPercentage = 0.5F;

                    if (value.Count >= 2 && int.TryParse(value[1], out iIndex) == true)
                    {
                        this.SplitType = iIndex;
                    }

                    if (value.Count >= 3 && float.TryParse(value[2], out flPercentage) == true)
                    {
                        if (flPercentage < 0.0F || flPercentage > 1.0F)
                        {
                            this.TwoSplitterPercentage = 0.5F;
                        }
                        else
                        {
                            this.TwoSplitterPercentage = flPercentage;
                        }
                    }

                    if (value.Count >= 4 && float.TryParse(value[3], out flPercentage) == true)
                    {
                        if (flPercentage < 0.0F || flPercentage > 1.0F)
                        {
                            this.FourSplitterPercentage = 0.5F;
                        }
                        else
                        {
                            this.FourSplitterPercentage = flPercentage;
                        }
                    }
                }
            }
        }

        public PlayerListSettings()
        {
            this.SplitType = 1;
            this.TwoSplitterPercentage = 0.5F;
            this.FourSplitterPercentage = 0.5F;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PRoCon.Controls.ControlsEx
{
    class CodRichTextBox : RichTextBox
    {

        protected readonly Dictionary<string, Color> ChatTextColours;

        /// <summary>
        /// Buffer of information to append to the text box
        /// </summary>
        protected String AppendTextBuffer { get; set; }

        /// <summary>
        /// Lock used whenever we modify the append text buffer
        /// </summary>
        protected readonly Object AppendTextBufferLock = new Object();

        /// <summary>
        /// Timer to tick every 100 ms and flush the cache
        /// </summary>
        protected System.Threading.Timer AppendTextTimer { get; set; }

        /// <summary>
        /// A copy of the content in the text box for manipulation. It's more efficient for us
        /// to maintain a copy than it is to ever ask the control for copy.
        /// </summary>
        public String Content { get; private set; }

        /// <summary>
        /// Fired whenever content has been flushed to the control
        /// </summary>
        public event Action<Object, EventArgs> Flushed;

        public int LineLength { get; private set; }

        public CodRichTextBox() : base()
        {
            // Deviated a bit from cod4 because some of these colours suck.
            //^0 = Black
            //^1 = Maroon (PRoCon error message colour)
            //^2 = MediumSeaGreen
            //^3 = DarkOrange
            //^4 = RoyalBlue
            //^5 = Cornflower Blue
            //^6 = Dark Violet
            //^7 = Deep Pink (No one is judging you if you use this in your plugin =D )
            //^8 = Red
            //^9 = Grey
            // http://www.tayloredmktg.com/rgb/
            this.ChatTextColours = new Dictionary<string, Color> {
                {"^0", Color.Black},
                {"^1", Color.Maroon},
                {"^2", Color.MediumSeaGreen},
                {"^3", Color.DarkOrange},
                {"^4", Color.RoyalBlue},
                {"^5", Color.CornflowerBlue},
                {"^6", Color.DarkViolet},
                {"^7", Color.DeepPink},
                {"^8", Color.Red},
                {"^9", Color.Gray}
            };

            this.AppendTextBuffer = "";
            this.AppendTextTimer = new System.Threading.Timer(state => this.FlushBuffer(), null, 250, 250);
        }

        protected virtual void OnFlushed()
        {
            var handler = Flushed;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void SetColour(string variable, string value)
        {

            string caretNumber = variable.Replace("TEXT_COLOUR_", "^");

            if (this.ChatTextColours.ContainsKey(caretNumber) == true)
            {
                this.ChatTextColours[caretNumber] = Color.FromName(value);
            }
            else
            {
                this.ChatTextColours.Add(caretNumber, Color.FromName(value));
            }

        }

        public int PopFirstLine()
        {
            int firstLineLength = 0;

            for (; firstLineLength < this.Content.Length; firstLineLength++)
            {
                if (this.Content[firstLineLength] == '\n')
                {
                    break;
                }
            }
            this.Content = this.Content.Remove(0, firstLineLength + 1);
            // this.Content = this.Content.Substring(firstLineLength + 1);

            this.LineLength--;

            return firstLineLength;
        }

        private void InternalAppend(string content)
        {
            this.LineLength += content.Count(c => c == '\n');

            this.Content += content;
        }

        public void TrimLines(int maxLines)
        {

            this.ReadOnly = false;

            int consoleBoxLines = this.LineLength;

            if ((consoleBoxLines > maxLines && this.Focused == false) || consoleBoxLines > 3000)
            {

                for (int i = 0; i < consoleBoxLines - maxLines; i++)
                {
                    this.Select(0, this.PopFirstLine());

                    this.SelectedText = String.Empty;
                }
            }

            this.ReadOnly = true;

        }

        private static int FindCaretCode(string text, int appendedLength)
        {

            int index = -1;

            for (int i = text.Length - appendedLength; i < text.Length - 1; i++)
            {
                if (text[i] == '^' && (char.IsDigit(text[i + 1]) == true || text[i + 1] == 'b' || text[i + 1] == 'n' || text[i + 1] == 'i'))
                {
                    if (i > 0 && text[i - 1] != '^')
                    {
                        index = i;
                        break;
                    }

                    if (i == 0)
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index;
        }

        private static int GetCaretCount(string text)
        {
            return text.Count(t => t == '^');
        }

        private struct STextChange
        {
            public int Position;
            public Color Colour;
            public Font Font;
        }

        protected void FlushBuffer()
        {
            String text = null;
            lock (this.AppendTextBufferLock)
            {
                text = this.AppendTextBuffer;

                this.AppendTextBuffer = "";
            }

            if (String.IsNullOrEmpty(text) == false)
            {
                this.InvokeIfRequired(() =>
                {

                    List<STextChange> changes = new List<STextChange>();

                    int carets = 0;
                    int appendedStartPosition = -1;
                    int appendedTextLength = -1;

                    if ((carets = GetCaretCount(text)) > 0)
                    {

                        appendedStartPosition = this.Text.Length;
                        appendedTextLength = text.Length;

                        int i = -1;
                        int foundCarets = 0;
                        bool findingColourCodes = false;

                        string colourCode = String.Empty;

                        do
                        {
                            i = -1;
                            findingColourCodes = false;

                            //if ((i = this.Find("^", this.Text.Length - iConsoleOutputLength - 1, this.Text.Length, RichTextBoxFinds.MatchCase)) > 0) {
                            if ((i = FindCaretCode(text, appendedTextLength)) >= 0)
                            {

                                if (i < appendedTextLength - 1 && char.IsDigit(text[i + 1]) == true)
                                {

                                    STextChange change = new STextChange
                                    {
                                        Position = i
                                    };

                                    colourCode = text.Substring(i, 2);

                                    // Remove the ^[0-9]
                                    text = text.Substring(0, i) + text.Substring(i + 2);
                                    appendedTextLength -= 2;

                                    if (this.ChatTextColours.ContainsKey(colourCode) == true)
                                    {
                                        change.Colour = this.ChatTextColours[colourCode];
                                    }

                                    changes.Add(change);

                                    findingColourCodes = true;
                                }
                                else
                                {
                                    char fontCode = 'n';
                                    if (i < appendedTextLength - 1 && ((fontCode = text[i + 1]) == 'b' || text[i + 1] == 'n' || text[i + 1] == 'i'))
                                    {

                                        STextChange stcChange = new STextChange
                                        {
                                            Position = i
                                        };

                                        switch (fontCode)
                                        {
                                            case 'n':
                                                stcChange.Font = this.Font;// new Font("Calibri", 10);
                                                break;
                                            case 'b':
                                                stcChange.Font = new Font(this.Font, FontStyle.Bold);  //new Font("Calibri", 10, FontStyle.Bold);
                                                break;
                                            case 'i':
                                                stcChange.Font = new Font(this.Font, FontStyle.Italic);  //new Font("Calibri", 10, FontStyle.Italic);
                                                break;
                                        }

                                        // Remove the ^[b|n|i]
                                        text = text.Substring(0, i) + text.Substring(i + 2);
                                        appendedTextLength -= 2;

                                        changes.Add(stcChange);

                                        findingColourCodes = true;
                                    }
                                }

                                // Just stops that last pass of the string when we know how many times
                                // it should pass anyway.
                                foundCarets++;
                            }
                        } while (findingColourCodes == true && foundCarets < carets);

                        while ((i = text.IndexOf("^^", System.StringComparison.Ordinal)) > 0)
                        {
                            text = text.Substring(0, i) + "^" + text.Substring(i + 2);
                            appendedTextLength--;
                        }
                    }

                    this.InternalAppend(text);
                    base.AppendText(text);

                    if (appendedStartPosition >= 0)
                    {
                        foreach (STextChange change in changes)
                        {
                            this.Select(appendedStartPosition + change.Position, appendedTextLength - change.Position);

                            if (change.Font != null)
                            {
                                this.SelectionFont = change.Font;
                            }

                            this.SelectionColor = change.Colour;
                        }
                    }

                    this.OnFlushed();
                });
            }
        }

        public new void AppendText(string text)
        {
            lock (this.AppendTextBufferLock)
            {
                this.AppendTextBuffer += "^0" + text;
            }
        }
    }
}

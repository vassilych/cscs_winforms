using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;
using SplitAndMerge;

namespace WindowsFormsCSCS
{
    public class CSCS_GUI
    {
        public static Form1 TheForm { get; set; }
        public static Dictionary<string, Control> Controls { get; set; } = new Dictionary<string, Control>();
        //public static Action<string, string> OnWidgetClick;

        static Dictionary<string, string> s_actionHandlers      = new Dictionary<string, string>();
        static Dictionary<string, string> s_preActionHandlers   = new Dictionary<string, string>();
        static Dictionary<string, string> s_doubleClickHandlers = new Dictionary<string, string>();
        static Dictionary<string, string> s_keyDownHandlers     = new Dictionary<string, string>();
        static Dictionary<string, string> s_keyUpHandlers       = new Dictionary<string, string>();
        static Dictionary<string, string> s_keyPressHandlers    = new Dictionary<string, string>();
        static Dictionary<string, string> s_textChangedHandlers = new Dictionary<string, string>();
        static Dictionary<string, string> s_mouseHoverHandlers  = new Dictionary<string, string>();

        static Dictionary<string, TabPage> s_tabPages           = new Dictionary<string, TabPage>();
        static TabControl s_tabControl;


        public static void Init()
        {
            ParserFunction.RegisterFunction("OpenFile",           new OpenFileFunction());
            ParserFunction.RegisterFunction("SaveFile",           new SaveFileFunction());

            ParserFunction.RegisterFunction("PickColor",          new PickColorFunction());
            ParserFunction.RegisterFunction("SetColor",           new ColorWidgetFunction());

            ParserFunction.RegisterFunction("AddWidget",          new AddWidgetFunction());
            ParserFunction.RegisterFunction("RemoveWidget",       new RemoveWidgetFunction());
            ParserFunction.RegisterFunction("AddTab",             new AddTabFunction());
            ParserFunction.RegisterFunction("RemoveTab",          new RemoveTabFunction());

            ParserFunction.RegisterFunction("MoveWidget",         new MoveWidgetFunction());
            ParserFunction.RegisterFunction("ShowWidget",         new ShowHideWidgetFunction(true));
            ParserFunction.RegisterFunction("HideWidget",         new ShowHideWidgetFunction(false));
            ParserFunction.RegisterFunction("ResizeWidget",       new ResizeWidgetFunction());
            ParserFunction.RegisterFunction("GetWidgetSize",      new GetWidgetSizeFunction());

            ParserFunction.RegisterFunction("GetText",            new GetTextWidgetFunction());
            ParserFunction.RegisterFunction("SetText",            new SetTextWidgetFunction());
            ParserFunction.RegisterFunction("AddWidgetData",      new AddWidgetDataFunction());

            ParserFunction.RegisterFunction("ChangeCursor",       new ChangeCursorFunction());
            ParserFunction.RegisterFunction("MessageBox",         new MessageBoxFunction());

            AddActions();
        }

        public static void RunScript(string fileName)
        {
            Init();

            string script = Utils.GetFileContents(fileName);
            Variable result = null;
            try
            {
                result = Interpreter.Instance.Process(script, fileName);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc.Message);
                Console.WriteLine(exc.StackTrace);
                ParserFunction.InvalidateStacksAfterLevel(0);
                throw;
            }
        }

        public static void AddActions()
        {
            CacheControls();
            foreach (KeyValuePair<string, Control> entry in Controls)
            {
                AddActions(entry.Value);
            }
        }

        public static void AddActions(Control control, string name = "")
        {
            if (control == null)
            {
                return;
            }
            name = string.IsNullOrWhiteSpace(name) ? control.Name : name;

            string clickAction       = name + "_Clicked";
            string preClickAction    = name + "_PreClicked";
            string doubleClickAction = name + "_DoubleClicked";
            string keyDownAction     = name + "_KeyDown";
            string keyUpAction       = name + "_KeyUp";
            string keyPressAction    = name + "_KeyPress";
            string textChangeAction  = name + "_TextChange";
            string mouseHoverAction  = name + "_MouseHover";

            AddActionHandler(     control.Name, clickAction, control);
            AddPreActionHandler(  control.Name, preClickAction, control);
            AddDoubleClickHandler(control.Name, doubleClickAction, control);
            AddKeyDownHandler(    control.Name, keyDownAction, control);
            AddKeyUpHandler(      control.Name, keyUpAction, control);
            AddKeyPressHandler(   control.Name, keyPressAction, control);
            AddTextChangedHandler(control.Name, textChangeAction, control);
            AddMouseHoverHandler( control.Name, mouseHoverAction, control);
        }

        public static Control GetWidget(string name)
        {
            CacheControls();
            Control result;
            if (Controls.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }

        public static string GetWidgetName(string text, bool isTabName = false)
        {
            CacheControls();
            foreach (KeyValuePair<string, Control> entry in Controls)
            {
                if (isTabName && !(entry.Value is TabPage))
                {
                    continue;
                }
                if (entry.Value.Text == text || entry.Value.Name == text)
                {
                    return entry.Key;
                }
            }
            return text;
        }

        public static void CacheControls(bool force = false)
        {
            if ((!force && Controls.Count > 0) || TheForm == null)
            {
                return;
            }

            foreach (var item in TheForm.Controls)
            {
                Control control = item as Control;
                Controls[control.Name] = control;
                if (control is TabControl)
                {
                    s_tabControl = control as TabControl;
                    foreach (var page in s_tabControl.TabPages)
                    {
                        var pageControl = page as TabPage;
                        s_tabPages[pageControl.Name] = pageControl;
                        Controls[pageControl.Name] = pageControl;
                        foreach (var itemi in pageControl.Controls)
                        {
                            var controli = itemi as Control;
                            if (controli != null)
                            {
                                Controls[controli.Name] = controli;
                            }
                        }
                    }
                }
            }
        }

        public static bool AddTab(string tabName)
        {
            if (s_tabControl == null || string.IsNullOrWhiteSpace(tabName))
            {
                return false;
            }
            TabPage tabPage = new TabPage(tabName);

            tabPage.Name = "tabPage" + (s_tabPages.Count + 1);
            if (s_tabPages.Count > 0)
            {
                var template = s_tabPages["tabPage" + s_tabPages.Count];
                tabPage.Padding = template.Padding;
                tabPage.Size = template.Size;
                tabPage.TabIndex = template.TabIndex + 1;
                tabPage.UseVisualStyleBackColor = template.UseVisualStyleBackColor;
            }
            else
            {
                tabPage.Location = new System.Drawing.Point(4, 25);
                tabPage.Padding = new System.Windows.Forms.Padding(3);
                tabPage.Size = new System.Drawing.Size(1047, 509);
                tabPage.TabIndex = 0;
                tabPage.UseVisualStyleBackColor = true;
            }

            s_tabPages[tabPage.Name] = tabPage;
            s_tabControl.Controls.Add(tabPage);
            Controls[tabPage.Name] = tabPage;

            return true;
        }

        public static bool RemoveTab(string tabName)
        {
            if (s_tabControl == null || string.IsNullOrWhiteSpace(tabName))
            {
                return false;
            }

            tabName = GetWidgetName(tabName, true);
            TabPage tabPage;
            if (!s_tabPages.TryGetValue(tabName, out tabPage))
            {
                return false;
            }

            foreach (var item in tabPage.Controls)
            {
                var control = item as Control;
                if (control != null)
                {
                    RemoveWidget(control.Name);
                }
            }

            s_tabPages.Remove(tabName);
            s_tabControl.Controls.Remove(tabPage);
            Controls.Remove(tabName);
            return true;
        }

        public static bool AddActionHandler(string name, string action, Control widget)
        {
            s_actionHandlers[name] = action;
            widget.Click += new System.EventHandler(Widget_Click);
            return true;
        }
        public static bool AddPreActionHandler(string name, string action, Control widget)
        {
            s_preActionHandlers[name] = action;
            widget.MouseDown += new MouseEventHandler(Widget_PreClick);
            return true;
        }
        public static bool AddDoubleClickHandler(string name, string action, Control widget)
        {
            s_doubleClickHandlers[name] = action;
            widget.DoubleClick += new System.EventHandler(Widget_DoubleClick);
            return true;
        }
        public static bool AddKeyDownHandler(string name, string action, Control widget)
        {
            s_keyDownHandlers[name] = action;
            widget.KeyDown += new KeyEventHandler(Widget_KeyDown);
            return true;
        }
        public static bool AddKeyUpHandler(string name, string action, Control widget)
        {
            s_keyUpHandlers[name] = action;
            widget.KeyUp += new KeyEventHandler(Widget_KeyUp);
            return true;
        }
        public static bool AddKeyPressHandler(string name, string action, Control widget)
        {
            s_keyPressHandlers[name] = action;
            widget.KeyPress += new KeyPressEventHandler(Widget_KeyPress);
            return true;
        }
        public static bool AddTextChangedHandler(string name, string action, Control widget)
        {
            s_textChangedHandlers[name] = action;
            widget.TextChanged += new EventHandler(Widget_TextChanged);
            return true;
        }
        public static bool AddMouseHoverHandler(string name, string action, Control widget)
        {
            s_mouseHoverHandlers[name] = action;
            widget.MouseHover += new EventHandler(Widget_Hover);
            return true;
        }

        private static void Widget_Click(object sender, EventArgs e)
        {
            Control widget = sender as Control;
            if (widget == null)
            {
                return;
            }
            string funcName;
            if (!s_actionHandlers.TryGetValue(widget.Name, out funcName))
            {
                return;
            }

            Variable result = null;
            if (widget is CheckBox)
            {
                result = new Variable(((CheckBox)widget).Checked);
            }
            else
            {
                result = new Variable(widget.Text);
            }
            CustomFunction.Run(funcName, new Variable(widget.Name), result);
        }

        private static void Widget_Hover(object sender, EventArgs e)
        {
            Control widget = sender as Control;
            if (widget == null)
            {
                return;
            }

            string funcName;
            if (s_mouseHoverHandlers.TryGetValue(widget.Name, out funcName))
            {
                CustomFunction.Run(funcName, new Variable(widget.Name), new Variable(e.ToString()));
            }
        }
        private static void Widget_PreClick(object sender, MouseEventArgs e)
        {
            Control widget = sender as Control;
            if (widget == null || e.Button != MouseButtons.Left)
            {
                return;
            }

            string funcName;
            if (s_preActionHandlers.TryGetValue(widget.Name, out funcName))
            {
                CustomFunction.Run(funcName, new Variable(widget.Name), new Variable(e.ToString()));
            }
        }
        private static void Widget_DoubleClick(object sender, EventArgs e)
        {
            Control widget = sender as Control;
            if (widget == null)
            {
                return;
            }

            string funcName;
            if (s_doubleClickHandlers.TryGetValue(widget.Name, out funcName))
            {
                CustomFunction.Run(funcName, new Variable(widget.Name), new Variable(e.ToString()));
            }
        }
        private static void Widget_KeyDown(object sender, KeyEventArgs e)
        {
            Control widget = sender as Control;            
            if (widget == null)
            {
                return;
            }

            string funcName;
            if (s_keyDownHandlers.TryGetValue(widget.Name, out funcName))
            {
                CustomFunction.Run(funcName, new Variable(widget.Name), new Variable(((char)e.KeyValue).ToString()));
            }
        }
        private static void Widget_KeyUp(object sender, KeyEventArgs e)
        {
            Control widget = sender as Control;
            if (widget == null)
            {
                return;
            }

            string funcName;
            if (s_keyUpHandlers.TryGetValue(widget.Name, out funcName))
            {
                CustomFunction.Run(funcName, new Variable(widget.Name), new Variable(((char)e.KeyValue).ToString()));
            }
        }
        private static void Widget_KeyPress(object sender, KeyPressEventArgs e)
        {
            Control widget = sender as Control;
            if (widget == null)
            {
                return;
            }

            string funcName;
            if (s_keyPressHandlers.TryGetValue(widget.Name, out funcName))
            {
                CustomFunction.Run(funcName, new Variable(widget.Name), new Variable(e.KeyChar.ToString()));
            }
        }
        private static void Widget_TextChanged(object sender, EventArgs e)
        {
            Control widget = sender as Control;
            if (widget == null)
            {
                return;
            }

            string funcName;
            if (s_textChangedHandlers.TryGetValue(widget.Name, out funcName))
            {
                CustomFunction.Run(funcName, new Variable(widget.Name), new Variable(widget.Text));
            }
        }
        public static void AddWidget(Control control, string widgetName, string callback = "")
        {
            CacheControls();

            if (s_tabControl != null && s_tabPages.Count > 0)
            {
                int index = widgetName.IndexOf('.');
                var widgetTab = index > 0 && index < widgetName.Length - 1 ? widgetName.Substring(0, index) : "";
                widgetName = index > 0 && index < widgetName.Length - 1 ? widgetName.Substring(index + 1) : widgetName;
                control.Name = widgetName;

                TabPage tabPage;
                if (string.IsNullOrEmpty(widgetTab) || !s_tabPages.TryGetValue(widgetTab, out tabPage))
                {
                    tabPage = s_tabControl.TabPages[0];
                }
                tabPage.Controls.Add(control);

            }
            else
            {
                control.Name = widgetName;
                TheForm.Controls.Add(control);
            }

            Controls[widgetName] = control;
            CSCS_GUI.AddActions(control, callback);
        }

        public static bool RemoveWidget(string widgetName)
        {
            Control control;
            if (s_tabControl != null && s_tabPages.Count > 0)
            {
                int index = widgetName.IndexOf('.');
                var widgetTab = index > 0 && index < widgetName.Length - 1 ? widgetName.Substring(0, index) : "";
                widgetName = index > 0 && index < widgetName.Length - 1 ? widgetName.Substring(index + 1) : widgetName;

                TabPage tabPage;
                if (string.IsNullOrEmpty(widgetTab) || !s_tabPages.TryGetValue(widgetTab, out tabPage))
                {
                    tabPage = s_tabControl.TabPages[0];
                }
                control = CSCS_GUI.GetWidget(widgetName);
                if (control == null)
                {
                    return false;
                }
                tabPage.Controls.Remove(control);

            }
            else
            {
                control = CSCS_GUI.GetWidget(widgetName);
                if (control == null)
                {
                    return false;
                }
                TheForm.Controls.Remove(control);
            }

            s_actionHandlers.Remove(widgetName);
            Controls.Remove(widgetName);
            control.Dispose();

            return true;
        }
    }

    class ChangeCursorFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            string cursorType = Utils.GetSafeString(args, 0, "ok");
            Application.UseWaitCursor = false;

            switch (cursorType)
            {
                case "ok":
                    Cursor.Current = Cursors.Default;
                    break;
                case "busy":
                    Cursor.Current = Cursors.WaitCursor;
                    Application.UseWaitCursor = true;
                    break;
                case "hand":
                    Cursor.Current = Cursors.Hand;
                    break;
                case "help":
                    Cursor.Current = Cursors.Help;
                    break;
                case "cross":
                    Cursor.Current = Cursors.Cross;
                    break;
                case "sizeall":
                    Cursor.Current = Cursors.SizeAll;
                    break;
                case "hsplit":
                    Cursor.Current = Cursors.HSplit;
                    break;
                case "vsplit":
                    Cursor.Current = Cursors.VSplit;
                    break;
                case "uparrow":
                    Cursor.Current = Cursors.UpArrow;
                    break;
                default:
                    Cursor.Current = Cursors.Default;
                    break;
            }

            return Variable.EmptyInstance;
        }
    }
    class OpenFileFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            /*List<Variable> args =*/ script.GetFunctionArgs();
            return OpenFile();
        }
        public static Variable OpenFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() != DialogResult.OK)
            {
                return Variable.EmptyInstance;
            }

            var fileName = openFile.FileName;
            string contents = Utils.GetFileContents(fileName);
            contents = contents.Replace("\n", Environment.NewLine);
            return new Variable(contents);
        }
    }
    class SaveFileFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            string text = Utils.GetSafeString(args, 0);

            return SaveFile(text);
        }
        public static Variable SaveFile(string text)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            if (saveFile.ShowDialog() != DialogResult.OK)
            {
                return Variable.EmptyInstance;
            }

            var fileName = saveFile.FileName;
            File.WriteAllText(fileName, text);
            return new Variable(fileName);
        }
    }

    class MessageBoxFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            var message = Utils.GetSafeString(args, 0);
            var caption = Utils.GetSafeString(args, 1, "Question");
            var messageType = Utils.GetSafeString(args, 2, "ok").ToLower();

            MessageBoxButtons buttons = messageType == "ok" ? MessageBoxButtons.OK :
                messageType == "okcancel"    ? MessageBoxButtons.OKCancel :
                messageType == "yesno"       ? MessageBoxButtons.YesNo :
                messageType == "yesnocancel" ? MessageBoxButtons.YesNoCancel :
                messageType == "retrycancel" ? MessageBoxButtons.RetryCancel : 
                                               MessageBoxButtons.AbortRetryIgnore;

            var result = MessageBox.Show(message, caption,
                                         buttons,
                                         MessageBoxIcon.Question);

            var ret = result == DialogResult.OK ? "Ok" :
                      result == DialogResult.Cancel ? "Cancel" :
                      result == DialogResult.Yes ? "Yes" :
                      result == DialogResult.No ? "No" :
                      result == DialogResult.Ignore ? "Ignore" :
                      result == DialogResult.Abort ? "Abort" :
                      result == DialogResult.Retry ? "Retry" : "None";

            return new Variable(ret);
        }
    }

    class GetTextWidgetFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            var widget = CSCS_GUI.GetWidget(widgetName);
            if (widget == null)
            {
                return Variable.EmptyInstance;
            }

            var result = widget.Text;
            if (widget is CheckBox)
            {
                var checkBox = widget as CheckBox;
                result = checkBox.Checked ? "true" : "false";
            }

            return new Variable(result);
        }
    }

    class SetTextWidgetFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            var text = Utils.GetSafeString(args, 1);

            var widget = CSCS_GUI.GetWidget(widgetName);
            if (widget == null)
            {
                return Variable.EmptyInstance;
            }

            if (widget is ComboBox)
            {
                var combo = widget as ComboBox;
                var index = 0;
                if (args[0].Type == Variable.VarType.NUMBER)
                {
                    index = (int)args[0].Value;
                }
                else
                {
                    foreach (var item in combo.Items)
                    {
                        if (item.ToString() == text)
                        {
                            break;
                        }
                        index++;
                    }
                }
                if (index >= 0 && index < combo.Items.Count)
                {
                    combo.SelectedIndex = index;
                }
            }
            widget.Text = text;

            return new Variable(true);
        }
    }

    class AddWidgetDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string widgetName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);
            var data = args[0];

            var widget = CSCS_GUI.GetWidget(widgetName);
            var itemsAdded = 0;
            if (widget is ComboBox)
            {
                var combo = widget as ComboBox;
                if (data.Type == Variable.VarType.ARRAY)
                {
                    foreach (var item in data.Tuple)
                    {
                        combo.Items.Add(item.AsString());
                    }
                    itemsAdded = data.Tuple.Count;
                }
                else
                {
                    combo.Items.Add(data.AsString());
                    itemsAdded = 1;
                }
            }

            return new Variable(itemsAdded);
        }
    }

    class AddWidgetFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 7, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            var widgetType = Utils.GetSafeString(args, 1).ToLower();
            var text       = Utils.GetSafeString(args, 2);
            var x          = Utils.GetSafeInt(args, 3);
            var y          = Utils.GetSafeInt(args, 4);
            var width      = Utils.GetSafeInt(args, 5);
            var height     = Utils.GetSafeInt(args, 6);
            var callback   = Utils.GetSafeString(args, 7);

            Control control = null;
            switch(widgetType)
            {
                case "button":
                    var button = new Button();
                    button.UseVisualStyleBackColor = true;
                    control = button;
                    break;
                case "checkbox":
                    var checkbox = new CheckBox();
                    control = checkbox;
                    break;
                case "label":
                    var label = new Label();
                    control = label;
                    break;
                case "textbox":
                    var textBox = new TextBox();
                    control = textBox;
                    break;
            }

            if (control == null)
            {
                return Variable.EmptyInstance;
            }

            control.Location = new System.Drawing.Point(x, y);
            control.Text = text;
            control.Size   = new System.Drawing.Size(width, height);

            CSCS_GUI.AddWidget(control, widgetName, callback);

            return new Variable(true);
        }
    }

    class RemoveWidgetFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            bool removed = CSCS_GUI.RemoveWidget(widgetName);

            return new Variable(removed);
        }
    }

    class AddTabFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            var tabName = Utils.GetSafeString(args, 0);
            bool added = CSCS_GUI.AddTab(tabName);

            return new Variable(added);
        }
    }

    class RemoveTabFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            var tabName = Utils.GetSafeString(args, 0);
            bool added = CSCS_GUI.RemoveTab(tabName);

            return new Variable(added);
        }
    }

    class ShowHideWidgetFunction : ParserFunction
    {
        bool m_showWidget;

        public ShowHideWidgetFunction(bool showWidget)
        {
            m_showWidget = showWidget;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            var widget = CSCS_GUI.GetWidget(widgetName);
            if (widget == null)
            {
                return Variable.EmptyInstance;
            }            

            if (m_showWidget)
            {
                widget.Show();
            }
            else
            {
                widget.Hide();
            }

            return new Variable(true);
        }
    }

    class MoveWidgetFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            var x = Utils.GetSafeInt(args, 1);
            var y = Utils.GetSafeInt(args, 2);

            var widget = CSCS_GUI.GetWidget(widgetName);
            if (widget == null)
            {
                return Variable.EmptyInstance;
            }
            widget.Location = new Point(x, y);

            return new Variable(true);
        }
    }

    class ResizeWidgetFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            var x = Utils.GetSafeInt(args, 1);
            var y = Utils.GetSafeInt(args, 2);

            var widget = CSCS_GUI.GetWidget(widgetName);
            if (widget == null)
            {
                return Variable.EmptyInstance;
            }
            widget.Size = new Size(x, y);

            return new Variable(true);
        }
    }

    class GetWidgetSizeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            var x = Utils.GetSafeInt(args, 1);
            var y = Utils.GetSafeInt(args, 2);

            var widget = CSCS_GUI.GetWidget(widgetName);
            if (widget == null)
            {
                return Variable.EmptyInstance;
            }

            var sizes = new List<double>();
            sizes.Add(widget.Size.Width);
            sizes.Add(widget.Size.Height);

            return new Variable(sizes);
        }
    }

    class ColorWidgetFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            var widgetName = Utils.GetSafeString(args, 0);
            var widget = args[0];
            if (widget.Object != null && widget.Object is Control)
            {
                widgetName = ((Control)widget.Object).Name;
            }
            int argb = Utils.GetSafeInt(args, 1);

            return SetColor(widgetName, argb);
        }
        public static Variable SetColor(string widgetName, int argb)
        {
            var widget = CSCS_GUI.GetWidget(widgetName);

            if (widget == null)
            {
                return Variable.EmptyInstance;
            }
            widget.BackColor = Color.FromArgb(argb);
            return new Variable(argb);
        }
    }

    class PickColorFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            /*List<Variable> args = */script.GetFunctionArgs();
            return PickColor();
        }
        public static Variable PickColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return Variable.EmptyInstance;
            }
            var color = colorDialog.Color;
            return new Variable(color.ToArgb());
        }
    }
}

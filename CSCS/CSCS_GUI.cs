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
        public static Form1 TheForm;

        public static void Init()
        {
            ParserFunction.RegisterFunction("OpenFile",         new OpenFileFunction());
            ParserFunction.RegisterFunction("SaveFile",         new SaveFileFunction());

            ParserFunction.RegisterFunction("AddButtonHandler", new AddButtonHandlerFunction());
            ParserFunction.RegisterFunction("PickColor",        new PickColorFunction());
            ParserFunction.RegisterFunction("SetColor",         new ColorWidgetFunction());

            ParserFunction.RegisterFunction("GetText",          new GetTextWidgetFunction());
            ParserFunction.RegisterFunction("SetText",          new SetTextWidgetFunction());

            ParserFunction.RegisterFunction("AddWidget",        new AddWidgetFunction());
            ParserFunction.RegisterFunction("MoveWidget",       new MoveWidgetFunction());
            ParserFunction.RegisterFunction("ResizeWidget",     new ResizeWidgetFunction());
            ParserFunction.RegisterFunction("GetWidgetSize",    new GetWidgetSizeFunction());

            ParserFunction.RegisterFunction("MessageBox",       new MessageBoxFunction());
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
    }

    class AddButtonHandlerFunction : ParserFunction
    {
        static Dictionary<string, string> s_buttonHandlers;

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string buttonName   = Utils.GetSafeString(args, 0);
            string buttonAction = Utils.GetSafeString(args, 1);

            AddButtonHandler(buttonName, buttonAction);
            return new Variable(buttonName);
        }

        public static void AddButtonHandler(string buttonName, string buttonAction)
        {
            if (s_buttonHandlers == null)
            {
                s_buttonHandlers = new Dictionary<string, string>();
                Form1.OnButtonClick += ButtonHandler;
            }

            s_buttonHandlers[buttonName] = buttonAction;
        }

        public static void ButtonHandler(string sender, string load)
        {
            string funcName;
            if (s_buttonHandlers.TryGetValue(sender, out funcName))
            {
                CustomFunction.Run(funcName, new Variable(sender), new Variable(load));
            }
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
            var widget = Form1.GetWidget(widgetName);
            if (widget == null)
            {
                return Variable.EmptyInstance;
            }
            return new Variable(widget.Text);
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

            var widget = Form1.GetWidget(widgetName);
            if (widget == null)
            {
                return Variable.EmptyInstance;
            }
            widget.Text = text;

            return new Variable(true);
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
                    if (!string.IsNullOrWhiteSpace(callback))
                    {
                        button.Click += new System.EventHandler(Button_Click);
                        AddButtonHandlerFunction.AddButtonHandler(widgetName, callback);
                    }
                    control = button;
                    break;
            }

            if (control == null)
            {
                return Variable.EmptyInstance;
            }

            control.Location = new System.Drawing.Point(x, y);
            control.Name = widgetName;
            control.Text = text;
            control.Size   = new System.Drawing.Size(width, height);
            control.Click += new System.EventHandler(Button_Click);

            CSCS_GUI.TheForm.Controls.Add(control);
            return new Variable(true);
        }

        private static void Button_Click(object sender, EventArgs e)
        {
            Form1.OnButtonClick?.Invoke(((System.Windows.Forms.Button)sender).Name, "button");
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

            var widget = Form1.GetWidget(widgetName);
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

            var widget = Form1.GetWidget(widgetName);
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

            var widget = Form1.GetWidget(widgetName);
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
            int argb = Utils.GetSafeInt(args, 1);

            return SetColor(widgetName, argb);
        }
        public static Variable SetColor(string widgetName, int argb)
        {
            var widget = Form1.GetWidget(widgetName);

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

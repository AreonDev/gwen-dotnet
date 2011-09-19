﻿using System;
using Gwen.Control;
using Gwen.Control.Layout;

namespace Gwen.UnitTest
{
    public class UnitTest : DockBase
    {
        private Control.Base m_LastControl;
        private readonly Control.StatusBar m_StatusBar;
        private readonly Control.ListBox m_TextOutput;
        private Control.TabControl m_TabControl;
        private Control.TabButton m_Button;
        private readonly Control.CollapsibleList m_List;

        public double Fps; // set this in your rendering loop

        public UnitTest(Base parent) : base(parent)
        {
            Dock = Pos.Fill;
            SetSize(1024, 768);
            m_List = new Control.CollapsibleList(this);

            LeftDock.TabControl.AddPage("Unit tests", m_List);
            LeftDock.Width = 150;

            m_TextOutput = new Control.ListBox(BottomDock);
            m_Button = BottomDock.TabControl.AddPage("Output", m_TextOutput);
            BottomDock.Height = 200;

            m_StatusBar = new Control.StatusBar(this);
            m_StatusBar.Dock = Pos.Bottom;

            Center center = new Center(this);
            center.Dock = Pos.Fill;
            GUnit test;

            {
                CollapsibleCategory cat = m_List.Add("Non-Interactive");
                {
                    test = new Label(center);
                    RegisterUnitTest("Label", cat, test);
                    test = new RichLabel(center);
                    RegisterUnitTest("RichLabel", cat, test);
                    test = new GroupBox(center);
                    RegisterUnitTest("GroupBox", cat, test);
                    test = new ProgressBar(center);
                    RegisterUnitTest("ProgressBar", cat, test);
                    test = new ImagePanel(center);
                    RegisterUnitTest("ImagePanel", cat, test);
                    test = new StatusBar(center);
                    RegisterUnitTest("StatusBar", cat, test);
                }
            }

            {
                CollapsibleCategory cat = m_List.Add("Standard");
                {
                    test = new Button(center);
                    RegisterUnitTest("Button", cat, test);
                    test = new TextBox(center);
                    RegisterUnitTest("TextBox", cat, test);
                    test = new CheckBox(center);
                    RegisterUnitTest("CheckBox", cat, test);
                    test = new RadioButton(center);
                    RegisterUnitTest("RadioButton", cat, test);
                    test = new ComboBox(center);
                    RegisterUnitTest("ComboBox", cat, test);
                    test = new ListBox(center);
                    RegisterUnitTest("ListBox", cat, test);
                    test = new NumericUpDown(center);
                    RegisterUnitTest("NumericUpDown", cat, test);
                    test = new Slider(center);
                    RegisterUnitTest("Slider", cat, test);
                    test = new MenuStrip(center);
                    RegisterUnitTest("MenuStrip", cat, test);
                    test = new CrossSplitter(center);
                    RegisterUnitTest("CrossSplitter", cat, test);
                }
            }
            
            {
                CollapsibleCategory cat = m_List.Add("Containers");
                {
                    test = new Window(center);
                    RegisterUnitTest("Window", cat, test);
                    test = new TreeControl(center);
                    RegisterUnitTest("TreeControl", cat, test);
                    test = new Properties(center);
                    RegisterUnitTest("Properties", cat, test);
                    test = new TabControl(center);
                    RegisterUnitTest("TabControl", cat, test);
                    test = new ScrollControl(center);
                    RegisterUnitTest("ScrollControl", cat, test);
                }
            }
            
            {
                CollapsibleCategory cat = m_List.Add("Non-standard");
                {
                    test = new CollapsibleList(center);
                    RegisterUnitTest("CollapsibleList", cat, test);
                    test = new ColorPickers(center);
                    RegisterUnitTest("Color pickers", cat, test);
                }
            }

            m_StatusBar.SendToBack();
            PrintText("Unit Test started!");
        }

        public void RegisterUnitTest(String name, CollapsibleCategory cat, GUnit test)
        {
            Control.Button btn = cat.Add(name);
            test.Dock = Pos.Fill;
            test.Hide();
            test.UnitTest = this;
            btn.UserData = test;
            btn.Clicked += onCategorySelect;
        }

        private void onCategorySelect(Base control)
        {
            if (m_LastControl != null)
            {
                m_LastControl.Hide();
            }
            Base test = control.UserData as Base;
            test.Show();
            m_LastControl = test;
        }

        public void PrintText(String str)
        {
            m_TextOutput.AddItem(str);
            m_TextOutput.ScrollToBottom();
        }

        protected override void Render(Skin.Base skin)
        {
            m_StatusBar.Text = String.Format("GWEN.Net Unit Test - {0:F0} fps", Fps);

            base.Render(skin);
        }
    }
}

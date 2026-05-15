using OpenRPA.Interfaces;
using System;
using System.Activities.Presentation.Toolbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenRPA.Views
{
    /// <summary>
    /// Interaction logic for Snippets.xaml
    /// </summary>
    public partial class Snippets : UserControl
    {
        public Snippets()
        {
            InitializeComponent();
            DataContext = this;
            toolborder.Child = toolbox;
        }
        public void Reload()
        {
            InitializeSnippets();
        }
        public ToolboxControl toolbox = new ToolboxControl();
        // public static DynamicActivityGenerator dag = new DynamicActivityGenerator("Snippets");
        public static DynamicActivityGenerator dag;
        public void InitializeSnippets()
        {
            try
            {
                if(dag == null)
                    dag = new DynamicActivityGenerator("Snippets", System.IO.Path.GetTempPath());

                var cs = new Dictionary<string, ToolboxCategory>();

                // Built-in snippet plugins
                foreach(var s in Plugins.Snippets)
                {
                    try
                    {
                        if (!cs.ContainsKey(s.Category)) cs.Add(s.Category, new ToolboxCategory(s.Category));
                        var t = dag.AppendSubWorkflowTemplate(s.Name, s.Xaml);
                        cs[s.Category].Add(new ToolboxItemWrapper(t, s.Name));
                    }
                    catch (Exception ex) { Log.Error(ex.ToString()); }
                }

                // User projects as categories
                foreach (var project in RobotInstance.instance.Projects)
                {
                    if (!cs.ContainsKey(project.name))
                        cs.Add(project.name, new ToolboxCategory(project.name));
                    foreach (var wf in project.Workflows)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(wf.Xaml)) continue;
                            var t = dag.AppendSubWorkflowTemplate(project.name + "_" + wf.name, wf.Xaml);
                            cs[project.name].Add(new ToolboxItemWrapper(t, wf.name));
                        }
                        catch (Exception ex) { Log.Error(ex.ToString()); }
                    }
                }

                try { dag.Save(); } catch (Exception ex) { Log.Error(ex.ToString()); }

                // Add new categories (don't remove existing)
                foreach (var c in cs)
                {
                    if (!toolbox.Categories.Any(x => x.CategoryName == c.Key))
                        toolbox.Categories.Add(c.Value);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                MessageBox.Show("InitializeSnippets: " + ex.Message);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }
    }
}

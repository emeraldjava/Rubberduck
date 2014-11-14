﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Vbe.Interop;

namespace Rubberduck.ToDoItems
{
    public partial class ToDoItemsControl : UserControl
    {
        private VBE vbe;
        private BindingList<ToDoItem> taskList;

        public ToDoItemsControl(VBE vbe)
        {
            this.vbe = vbe;

            InitializeComponent();
            
            RefreshTaskList();
            InitializeGrid();

        }

        private void InitializeGrid()
        {
            todoItemsGridView.DataSource = taskList;
            var descriptionColumn = todoItemsGridView.Columns["Description"];
            if (descriptionColumn != null)
            {
                descriptionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            todoItemsGridView.CellDoubleClick += taskListGridView_CellDoubleClick;
            refreshButton.Click += refreshButton_Click;

        }

        void taskListGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ToDoItem task = taskList.ElementAt(e.RowIndex);
            VBComponent component = vbe.ActiveVBProject.VBComponents.Item(task.Module);

            component.CodeModule.CodePane.Show();
            component.CodeModule.CodePane.SetSelection(task.LineNumber, 1, task.LineNumber, 1);
        }

        private void RefreshGridView()
        {
            RefreshTaskList();
            todoItemsGridView.DataSource = taskList;
            todoItemsGridView.Refresh();
        }

        public void RefreshTaskList()
        {
            this.taskList = new BindingList<ToDoItem>();

            foreach (VBComponent component in this.vbe.ActiveVBProject.VBComponents)
            {
                CodeModule module = component.CodeModule;
                for (var i = 1; i <= module.CountOfLines; i++)
                {
                    string line = module.get_Lines(i, 1);
                    if (IsTaskComment(line))
                    {
                        var priority = GetTaskPriority(line);
                        this.taskList.Add(new ToDoItem(priority, line, module, i));
                    }
                }
            }
        }

        private TaskPriority GetTaskPriority(string line)
        {
            //todo: Create xml config file to allow user customization of tags
            var upCasedLine = line.ToUpper();
            if (upCasedLine.Contains("'BUG:"))
            {
                return TaskPriority.High;
            }
            if(upCasedLine.Contains("'TODO:"))
            {
                return TaskPriority.Medium;
            }
            //must be a "NOTE:"
            return TaskPriority.Low;
        }

        private bool IsTaskComment(string line)
        {
            var upCasedLine = line.ToUpper();
            return (upCasedLine.Contains("'TODO:") || upCasedLine.Contains("'BUG:") || upCasedLine.Contains("'NOTE:"));
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshGridView();
        }
    }
}
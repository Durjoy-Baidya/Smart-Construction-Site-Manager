using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartConstructionSiteManagement;

public partial class DashboardForm
{
    private void ShowTasksContent()
    {
        ResetContentPanel();

        contentPanel.Controls.Add(CreateModuleTitle("Task Management"));

        RoundedPanel filterPanel = new()
        {
            Location = new Point(34, 98),
            Size = new Size(contentPanel.ClientSize.Width - 68, 70),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.White,
            BorderRadius = 4,
            BorderColor = BorderColor
        };
        contentPanel.Controls.Add(filterPanel);

        TextBox searchTextBox = new()
        {
            PlaceholderText = "Search tasks...",
            Font = new Font("Segoe UI", 11F),
            Size = new Size(480, 34),
            Left = 16,
            Top = 18
        };
        filterPanel.Controls.Add(searchTextBox);

        ComboBox statusComboBox = CreateFilterComboBox("All Status", 420);
        statusComboBox.Items.AddRange(["All Status", "Pending", "In Progress", "Completed", "Overdue"]);
        statusComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(statusComboBox);

        ComboBox priorityComboBox = CreateFilterComboBox("All Priority", 680);
        priorityComboBox.Items.AddRange(["All Priority", "High", "Medium", "Low"]);
        priorityComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(priorityComboBox);

        Button addButton = CreateActionButton("Add Task", PrimaryBlue, 0);
        addButton.Top = 18;
        addButton.Visible = CanManageTasks();
        filterPanel.Controls.Add(addButton);

        FlowLayoutPanel statPanel = new()
        {
            Location = new Point(34, 188),
            Size = new Size(contentPanel.ClientSize.Width - 68, 105),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = PageBackColor,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        contentPanel.Controls.Add(statPanel);

        RoundedPanel tasksTable = CreateWorkersTablePanel();
        contentPanel.Controls.Add(tasksTable);

        Label showingLabel = new()
        {
            Text = "Showing tasks",
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(260, 28),
            Left = 34,
            Top = 730
        };
        contentPanel.Controls.Add(showingLabel);

        void RefreshTaskStats()
        {
            statPanel.Controls.Clear();
            statPanel.Controls.Add(CreatePlainStatCard("Total Tasks", tasks.Count.ToString(), Color.FromArgb(52, 152, 219)));
            statPanel.Controls.Add(CreatePlainStatCard("In Progress", tasks.Count(task => task.Status == "In Progress").ToString(), Green));
            statPanel.Controls.Add(CreatePlainStatCard("Completed", tasks.Count(task => task.Status == "Completed").ToString(), Green));
            statPanel.Controls.Add(CreatePlainStatCard("Overdue", tasks.Count(task => task.Status == "Overdue").ToString(), Red));
        }

        void RefreshTasksTable()
        {
            List<TaskRecord> filteredTasks = GetFilteredTasks(searchTextBox.Text, statusComboBox.Text, priorityComboBox.Text);
            tasksTable.Height = GetManagementTableHeight(filteredTasks.Count);
            PopulateTasksTable(tasksTable, filteredTasks, RefreshTasksTable);
            showingLabel.Top = tasksTable.Bottom + 20;
            showingLabel.Text = $"Showing {filteredTasks.Count} of {tasks.Count} tasks";
            RefreshTaskStats();
        }

        void ArrangeTasksPage()
        {
            filterPanel.Width = contentPanel.ClientSize.Width - 68;
            int rightEdge = filterPanel.Width - 16;
            if (addButton.Visible)
            {
                addButton.Left = rightEdge - addButton.Width;
                rightEdge = addButton.Left - 20;
            }

            priorityComboBox.Left = rightEdge - priorityComboBox.Width;
            statusComboBox.Left = priorityComboBox.Left - statusComboBox.Width - 16;
            searchTextBox.Width = Math.Min(520, Math.Max(320, statusComboBox.Left - searchTextBox.Left - 28));
            statPanel.Width = contentPanel.ClientSize.Width - 68;
            List<TaskRecord> filteredTasks = GetFilteredTasks(searchTextBox.Text, statusComboBox.Text, priorityComboBox.Text);
            tasksTable.SetBounds(34, 315, contentPanel.ClientSize.Width - 68, GetManagementTableHeight(filteredTasks.Count));
            PopulateTasksTable(tasksTable, filteredTasks, RefreshTasksTable);
            showingLabel.Top = tasksTable.Bottom + 20;
        }

        searchTextBox.TextChanged += (_, _) => RefreshTasksTable();
        statusComboBox.SelectedIndexChanged += (_, _) => RefreshTasksTable();
        priorityComboBox.SelectedIndexChanged += (_, _) => RefreshTasksTable();
        if (CanManageTasks())
        {
            addButton.Click += (_, _) => AddTask(RefreshTasksTable);
        }
        SetContentPanelResizeHandler((_, _) => ArrangeTasksPage());
        RefreshTaskStats();
        ArrangeTasksPage();
        RefreshTasksTable();
    }

    private void PopulateTasksTable(RoundedPanel tablePanel, List<TaskRecord> filteredTasks, Action refreshTable)
    {
        tablePanel.SuspendLayout();
        tablePanel.Controls.Clear();

        int[] columnWidths = GetTaskColumnWidths(tablePanel.Width);
        string[] headers = CanManageTasks()
            ? ["Task Name", "Project", "Assigned To", "Priority", "Status", "Due Date", "Actions"]
            : ["Task Name", "Project", "Assigned To", "Priority", "Status", "Due Date"];

        Panel headerPanel = new()
        {
            BackColor = Color.FromArgb(243, 246, 250),
            Location = new Point(1, 1),
            Size = new Size(Math.Max(0, tablePanel.Width - 2), 54)
        };
        headerPanel.Paint += (_, e) =>
        {
            using Pen linePen = new(BorderColor, 1);
            e.Graphics.DrawLine(linePen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
        };
        tablePanel.Controls.Add(headerPanel);

        int left = 14;
        for (int i = 0; i < headers.Length; i++)
        {
            headerPanel.Controls.Add(CreateTableHeaderLabel(headers[i], left, columnWidths[i]));
            left += columnWidths[i];
        }
        AddTableRowSeparator(headerPanel);

        int rowTop = 55;
        foreach (TaskRecord task in filteredTasks)
        {
            tablePanel.Controls.Add(CreateTaskTableRow(task, rowTop, columnWidths, tablePanel.Width - 2, refreshTable));
            rowTop += 68;
        }

        tablePanel.ResumeLayout();
    }

    private int[] GetTaskColumnWidths(int tableWidth)
    {
        int contentWidth = Math.Max(1125, tableWidth - 28);
        int[] baseWidths = CanManageTasks()
            ? [240, 170, 170, 110, 130, 120, 185]
            : [300, 220, 220, 140, 155, 90];
        double scale = contentWidth / 1125.0;

        return baseWidths
            .Select(width => (int)Math.Round(width * scale))
            .ToArray();
    }

    private Panel CreateTaskTableRow(TaskRecord task, int top, int[] columnWidths, int rowWidth, Action refreshTable)
    {
        Panel rowPanel = new()
        {
            BackColor = Color.White,
            Location = new Point(1, top),
            Size = new Size(rowWidth, 68)
        };
        rowPanel.Paint += (_, e) =>
        {
            using Pen rowBorderPen = new(TableRowLineColor, 1);
            e.Graphics.DrawLine(rowBorderPen, 0, rowPanel.Height - 1, rowPanel.Width, rowPanel.Height - 1);
        };

        int left = 14;
        rowPanel.Controls.Add(CreateTableCell(task.Title, left, columnWidths[0]));
        left += columnWidths[0];
        rowPanel.Controls.Add(CreateTableCell(task.ProjectName, left, columnWidths[1]));
        left += columnWidths[1];
        rowPanel.Controls.Add(CreateTableCell(task.AssignedTo, left, columnWidths[2]));
        left += columnWidths[2];
        rowPanel.Controls.Add(CreatePriorityBadge(task.Priority, left + 4, 19));
        left += columnWidths[3];
        rowPanel.Controls.Add(CreateTaskStatusBadge(task.Status, left + 4, 19));
        left += columnWidths[4];
        rowPanel.Controls.Add(CreateTableCell(task.DueDateText, left, columnWidths[5]));
        left += columnWidths[5];

        if (CanManageTasks())
        {
            Button editButton = CreateRowActionButton("Edit", PrimaryBlue, left + 6);
            editButton.Click += (_, _) => EditTask(task, refreshTable);
            rowPanel.Controls.Add(editButton);

            Button deleteButton = CreateRowActionButton("Delete", Red, left + 76);
            deleteButton.Click += (_, _) => DeleteTask(task, refreshTable);
            rowPanel.Controls.Add(deleteButton);
        }

        AddTableRowSeparator(rowPanel);
        return rowPanel;
    }

    private static Label CreatePriorityBadge(string priority, int left, int top)
    {
        Color backColor = priority switch
        {
            "High" => Color.FromArgb(254, 226, 226),
            "Medium" => Color.FromArgb(255, 251, 235),
            _ => Color.FromArgb(229, 231, 235)
        };

        Color foreColor = priority switch
        {
            "High" => Color.FromArgb(153, 27, 27),
            "Medium" => Color.FromArgb(146, 64, 14),
            _ => Color.FromArgb(55, 65, 81)
        };

        return new Label
        {
            Text = priority,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = foreColor,
            BackColor = backColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(78, 30),
            Left = left,
            Top = top
        };
    }

    private Label CreateTaskStatusBadge(string status, int left, int top)
    {
        Color backColor = status switch
        {
            "Completed" => Color.FromArgb(220, 252, 231),
            "In Progress" => Color.FromArgb(219, 234, 254),
            "Pending" => Color.FromArgb(255, 251, 235),
            _ => Color.FromArgb(254, 226, 226)
        };

        Color foreColor = status switch
        {
            "Completed" => Color.FromArgb(22, 101, 52),
            "In Progress" => Color.FromArgb(30, 64, 175),
            "Pending" => Color.FromArgb(146, 64, 14),
            _ => Color.FromArgb(153, 27, 27)
        };

        return new Label
        {
            Text = status,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = foreColor,
            BackColor = backColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(status == "In Progress" ? 92 : 82, 30),
            Left = left,
            Top = top
        };
    }

    private List<TaskRecord> GetFilteredTasks(string searchText, string statusFilter, string priorityFilter)
    {
        string normalizedSearch = searchText.Trim().ToLowerInvariant();
        string selectedStatus = statusFilter.Replace("Status: ", "");
        string selectedPriority = priorityFilter.Replace("Priority: ", "");

        return tasks
            .Where(task =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                task.TaskCode.ToLowerInvariant().Contains(normalizedSearch) ||
                task.Title.ToLowerInvariant().Contains(normalizedSearch) ||
                task.ProjectName.ToLowerInvariant().Contains(normalizedSearch) ||
                task.AssignedTo.ToLowerInvariant().Contains(normalizedSearch))
            .Where(task => selectedStatus == "All" || selectedStatus == "All Status" || task.Status == selectedStatus)
            .Where(task => selectedPriority == "All" || selectedPriority == "All Priority" || task.Priority == selectedPriority)
            .ToList();
    }

    private void AddTask(Action refreshTable)
    {
        using TaskDialog dialog = new("Add Task", null, projects.Select(project => project.Name), workers.Select(worker => worker.Name));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Task == null)
        {
            return;
        }

        int nextId = tasks.Count == 0 ? 1 : tasks.Max(task => task.Id) + 1;
        dialog.Task.Id = nextId;
        dialog.Task.TaskCode = $"T{nextId:000}";
        tasks.Add(dialog.Task);
        SaveApplicationData();
        refreshTable();
    }

    private void EditTask(TaskRecord task, Action refreshTable)
    {
        using TaskDialog dialog = new("Edit Task", task, projects.Select(project => project.Name), workers.Select(worker => worker.Name));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Task == null)
        {
            return;
        }

        task.Title = dialog.Task.Title;
        task.ProjectName = dialog.Task.ProjectName;
        task.AssignedTo = dialog.Task.AssignedTo;
        task.Priority = dialog.Task.Priority;
        task.DueDate = dialog.Task.DueDate;
        task.Status = dialog.Task.Status;
        SaveApplicationData();
        refreshTable();
    }

    private void DeleteTask(TaskRecord task, Action refreshTable)
    {
        DialogResult result = MessageBox.Show(
            $"Delete task '{task.Title}'?",
            "Delete Task",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        tasks.Remove(task);
        SaveApplicationData();
        refreshTable();
    }
}

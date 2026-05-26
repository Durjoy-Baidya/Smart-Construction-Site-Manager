using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartConstructionSiteManagement;

public partial class DashboardForm
{
    private void ShowProjectsContent()
    {
        ResetContentPanel();

        contentPanel.Controls.Add(CreateModuleTitle("Projects Management"));

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
            PlaceholderText = "Search projects...",
            Font = new Font("Segoe UI", 11F),
            Size = new Size(480, 34),
            Left = 16,
            Top = 18
        };
        filterPanel.Controls.Add(searchTextBox);

        ComboBox statusComboBox = CreateFilterComboBox("All Status", 420);
        statusComboBox.Items.AddRange(["All Status", "Planning", "In Progress", "Completed", "Delayed"]);
        statusComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(statusComboBox);

        Button addButton = CreateActionButton("Add Project", PrimaryBlue, 0);
        addButton.Top = 18;
        addButton.Visible = CanManageProjects();
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

        RoundedPanel projectsTable = CreateWorkersTablePanel();
        contentPanel.Controls.Add(projectsTable);

        Label showingLabel = new()
        {
            Text = "Showing projects",
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(260, 28),
            Left = 34,
            Top = 730
        };
        contentPanel.Controls.Add(showingLabel);

        void RefreshProjectsTable()
        {
            List<ProjectRecord> filteredProjects = GetFilteredProjects(searchTextBox.Text, statusComboBox.Text);
            projectsTable.Height = GetManagementTableHeight(filteredProjects.Count);
            PopulateProjectsTable(projectsTable, filteredProjects, RefreshProjectsTable);
            showingLabel.Top = projectsTable.Bottom + 20;
            showingLabel.Text = $"Showing {filteredProjects.Count} of {projects.Count} projects";
            RefreshProjectStats();
        }

        void RefreshProjectStats()
        {
            statPanel.Controls.Clear();
            statPanel.Controls.Add(CreatePlainStatCard("Total Projects", projects.Count.ToString(), Color.FromArgb(52, 152, 219)));
            statPanel.Controls.Add(CreatePlainStatCard("In Progress", projects.Count(project => project.Status == "In Progress").ToString(), Green));
            statPanel.Controls.Add(CreatePlainStatCard("Completed", projects.Count(project => project.Status == "Completed").ToString(), Green));
            statPanel.Controls.Add(CreatePlainStatCard("Delayed", projects.Count(project => project.Status == "Delayed").ToString(), Red));
        }

        void ArrangeProjectsPage()
        {
            filterPanel.Width = contentPanel.ClientSize.Width - 68;
            int rightEdge = filterPanel.Width - 16;
            if (addButton.Visible)
            {
                addButton.Left = rightEdge - addButton.Width;
                rightEdge = addButton.Left - 20;
            }

            statusComboBox.Left = rightEdge - statusComboBox.Width;
            searchTextBox.Width = Math.Min(520, Math.Max(320, statusComboBox.Left - searchTextBox.Left - 28));
            statPanel.Width = contentPanel.ClientSize.Width - 68;
            List<ProjectRecord> filteredProjects = GetFilteredProjects(searchTextBox.Text, statusComboBox.Text);
            projectsTable.SetBounds(34, 315, contentPanel.ClientSize.Width - 68, GetManagementTableHeight(filteredProjects.Count));
            PopulateProjectsTable(projectsTable, filteredProjects, RefreshProjectsTable);
            showingLabel.Top = projectsTable.Bottom + 20;
        }

        searchTextBox.TextChanged += (_, _) => RefreshProjectsTable();
        statusComboBox.SelectedIndexChanged += (_, _) => RefreshProjectsTable();
        if (CanManageProjects())
        {
            addButton.Click += (_, _) => AddProject(RefreshProjectsTable);
        }
        SetContentPanelResizeHandler((_, _) => ArrangeProjectsPage());
        RefreshProjectStats();
        ArrangeProjectsPage();
        RefreshProjectsTable();
    }

    private void PopulateProjectsTable(RoundedPanel tablePanel, List<ProjectRecord> filteredProjects, Action refreshTable)
    {
        tablePanel.SuspendLayout();
        tablePanel.Controls.Clear();

        int[] columnWidths = GetProjectColumnWidths(tablePanel.Width);
        string[] headers = ["Project Name", "Location", "Start Date", "End Date", "Status", "Created By", "Actions"];

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
        foreach (ProjectRecord project in filteredProjects)
        {
            tablePanel.Controls.Add(CreateProjectTableRow(project, rowTop, columnWidths, tablePanel.Width - 2, refreshTable));
            rowTop += 68;
        }

        tablePanel.ResumeLayout();
    }

    private static int[] GetProjectColumnWidths(int tableWidth)
    {
        int contentWidth = Math.Max(1100, tableWidth - 28);
        int[] baseWidths = [220, 155, 130, 130, 125, 180, 160];
        double scale = contentWidth / 1100.0;

        return baseWidths
            .Select(width => (int)Math.Round(width * scale))
            .ToArray();
    }

    private Panel CreateProjectTableRow(ProjectRecord project, int top, int[] columnWidths, int rowWidth, Action refreshTable)
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
        rowPanel.Controls.Add(CreateTableCell(project.Name, left, columnWidths[0]));
        left += columnWidths[0];
        rowPanel.Controls.Add(CreateTableCell(project.Location, left, columnWidths[1]));
        left += columnWidths[1];
        rowPanel.Controls.Add(CreateTableCell(project.StartDateText, left, columnWidths[2]));
        left += columnWidths[2];
        rowPanel.Controls.Add(CreateTableCell(project.EndDateText, left, columnWidths[3]));
        left += columnWidths[3];
        rowPanel.Controls.Add(CreateProjectStatusBadge(project.Status, left + 4, 19));
        left += columnWidths[4];
        rowPanel.Controls.Add(CreateTableCell(project.Manager, left, columnWidths[5]));
        left += columnWidths[5];

        if (CanManageProjects())
        {
            Button editButton = CreateRowActionButton("Edit", PrimaryBlue, left + 6);
            editButton.Click += (_, _) => EditProject(project, refreshTable);
            rowPanel.Controls.Add(editButton);

            Button deleteButton = CreateRowActionButton("Delete", Red, left + 76);
            deleteButton.Click += (_, _) => DeleteProject(project, refreshTable);
            rowPanel.Controls.Add(deleteButton);
        }

        AddTableRowSeparator(rowPanel);
        return rowPanel;
    }

    private Label CreateProjectStatusBadge(string status, int left, int top)
    {
        Color backColor = status switch
        {
            "Completed" => Color.FromArgb(220, 252, 231),
            "In Progress" => Color.FromArgb(219, 234, 254),
            "Planning" => Color.FromArgb(255, 251, 235),
            _ => Color.FromArgb(254, 226, 226)
        };

        Color foreColor = status switch
        {
            "Completed" => Color.FromArgb(22, 101, 52),
            "In Progress" => Color.FromArgb(30, 64, 175),
            "Planning" => Color.FromArgb(146, 64, 14),
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

    private List<ProjectRecord> GetFilteredProjects(string searchText, string statusFilter)
    {
        string normalizedSearch = searchText.Trim().ToLowerInvariant();
        string selectedStatus = statusFilter.Replace("Status: ", "");

        return projects
            .Where(project =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                project.ProjectCode.ToLowerInvariant().Contains(normalizedSearch) ||
                project.Name.ToLowerInvariant().Contains(normalizedSearch) ||
                project.Location.ToLowerInvariant().Contains(normalizedSearch) ||
                project.Manager.ToLowerInvariant().Contains(normalizedSearch))
            .Where(project => selectedStatus == "All" || selectedStatus == "All Status" || project.Status == selectedStatus)
            .ToList();
    }

    private void AddProject(Action refreshTable)
    {
        using ProjectDialog dialog = new("Add Project", null, workers.Select(worker => worker.Name));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Project == null)
        {
            return;
        }

        int nextId = projects.Count == 0 ? 1 : projects.Max(project => project.Id) + 1;
        dialog.Project.Id = nextId;
        dialog.Project.ProjectCode = $"P{nextId:000}";
        projects.Add(dialog.Project);
        SaveApplicationData();
        refreshTable();
    }

    private void EditProject(ProjectRecord project, Action refreshTable)
    {
        using ProjectDialog dialog = new("Edit Project", project, workers.Select(worker => worker.Name));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Project == null)
        {
            return;
        }

        project.Name = dialog.Project.Name;
        project.Location = dialog.Project.Location;
        project.StartDate = dialog.Project.StartDate;
        project.EndDate = dialog.Project.EndDate;
        project.Manager = dialog.Project.Manager;
        project.Status = dialog.Project.Status;
        project.Progress = dialog.Project.Progress;
        SaveApplicationData();
        refreshTable();
    }

    private void DeleteProject(ProjectRecord project, Action refreshTable)
    {
        DialogResult result = MessageBox.Show(
            $"Delete project '{project.Name}'?",
            "Delete Project",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        projects.Remove(project);
        SaveApplicationData();
        refreshTable();
    }
}

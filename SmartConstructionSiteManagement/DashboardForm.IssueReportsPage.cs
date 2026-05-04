using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartConstructionSiteManagement;

public partial class DashboardForm
{
    private void ShowIssueReportsContent()
    {
        contentPanel.Controls.Clear();

        contentPanel.Controls.Add(CreateModuleTitle("Issue Reporting"));

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
            PlaceholderText = "Search issues...",
            Font = new Font("Segoe UI", 11F),
            Size = new Size(420, 34),
            Left = 16,
            Top = 18
        };
        filterPanel.Controls.Add(searchTextBox);

        ComboBox statusComboBox = CreateFilterComboBox("Status: All", 460);
        statusComboBox.Width = 190;
        statusComboBox.Items.AddRange(["Status: All", "Open", "In Progress", "Resolved"]);
        statusComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(statusComboBox);

        ComboBox priorityComboBox = CreateFilterComboBox("Priority: All", 670);
        priorityComboBox.Width = 190;
        priorityComboBox.Items.AddRange(["Priority: All", "High", "Medium", "Low"]);
        priorityComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(priorityComboBox);

        ComboBox projectComboBox = CreateFilterComboBox("Project: All", 880);
        projectComboBox.Width = 190;
        projectComboBox.Items.Add("Project: All");
        projectComboBox.Items.AddRange(projects.Select(project => project.Name).Distinct().ToArray());
        projectComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(projectComboBox);

        Button addButton = CreateActionButton("Report New Issue", PrimaryBlue, 0);
        addButton.Size = new Size(170, 36);
        addButton.Top = 18;
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

        RoundedPanel issuesTable = CreateWorkersTablePanel();
        contentPanel.Controls.Add(issuesTable);

        Label showingLabel = new()
        {
            Text = "Showing issues",
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(280, 28),
            Left = 34,
            Top = 730
        };
        contentPanel.Controls.Add(showingLabel);

        void RefreshIssueStats()
        {
            statPanel.Controls.Clear();
            statPanel.Controls.Add(CreatePlainStatCard("Total Issues", issueReports.Count.ToString(), Color.FromArgb(52, 152, 219)));
            statPanel.Controls.Add(CreatePlainStatCard("Open Issues", issueReports.Count(issue => issue.Status == "Open").ToString(), Orange));
            statPanel.Controls.Add(CreatePlainStatCard("In Progress", issueReports.Count(issue => issue.Status == "In Progress").ToString(), Color.FromArgb(52, 152, 219)));
            statPanel.Controls.Add(CreatePlainStatCard("Resolved", issueReports.Count(issue => issue.Status == "Resolved").ToString(), Green));
        }

        void RefreshIssuesTable()
        {
            List<IssueReportRecord> filteredIssues = GetFilteredIssueReports(searchTextBox.Text, statusComboBox.Text, priorityComboBox.Text, projectComboBox.Text);
            issuesTable.Height = GetManagementTableHeight(filteredIssues.Count);
            PopulateIssueReportsTable(issuesTable, filteredIssues, RefreshIssuesTable);
            showingLabel.Top = issuesTable.Bottom + 20;
            showingLabel.Text = $"Showing {filteredIssues.Count} of {issueReports.Count} issues";
            RefreshIssueStats();
        }

        void ArrangeIssueReportsPage()
        {
            filterPanel.Width = contentPanel.ClientSize.Width - 68;
            addButton.Left = filterPanel.Width - addButton.Width - 16;
            projectComboBox.Left = addButton.Left - projectComboBox.Width - 20;
            priorityComboBox.Left = projectComboBox.Left - priorityComboBox.Width - 16;
            statusComboBox.Left = priorityComboBox.Left - statusComboBox.Width - 16;
            searchTextBox.Width = Math.Min(420, Math.Max(260, statusComboBox.Left - searchTextBox.Left - 28));
            statPanel.Width = contentPanel.ClientSize.Width - 68;
            List<IssueReportRecord> filteredIssues = GetFilteredIssueReports(searchTextBox.Text, statusComboBox.Text, priorityComboBox.Text, projectComboBox.Text);
            issuesTable.SetBounds(34, 315, contentPanel.ClientSize.Width - 68, GetManagementTableHeight(filteredIssues.Count));
            PopulateIssueReportsTable(issuesTable, filteredIssues, RefreshIssuesTable);
            showingLabel.Top = issuesTable.Bottom + 20;
        }

        searchTextBox.TextChanged += (_, _) => RefreshIssuesTable();
        statusComboBox.SelectedIndexChanged += (_, _) => RefreshIssuesTable();
        priorityComboBox.SelectedIndexChanged += (_, _) => RefreshIssuesTable();
        projectComboBox.SelectedIndexChanged += (_, _) => RefreshIssuesTable();
        addButton.Click += (_, _) => AddIssueReport(RefreshIssuesTable);
        contentPanel.Resize += (_, _) => ArrangeIssueReportsPage();
        RefreshIssueStats();
        ArrangeIssueReportsPage();
        RefreshIssuesTable();
    }

    private void PopulateIssueReportsTable(RoundedPanel tablePanel, List<IssueReportRecord> filteredIssues, Action refreshTable)
    {
        tablePanel.SuspendLayout();
        tablePanel.Controls.Clear();

        int[] columnWidths = GetIssueColumnWidths(tablePanel.Width);
        string[] headers = ["Issue Title", "Project", "Reported By", "Priority", "Status", "Reported Date", "Actions"];

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
        foreach (IssueReportRecord issue in filteredIssues)
        {
            tablePanel.Controls.Add(CreateIssueReportTableRow(issue, rowTop, columnWidths, tablePanel.Width - 2, refreshTable));
            rowTop += 68;
        }

        tablePanel.ResumeLayout();
    }

    private static int[] GetIssueColumnWidths(int tableWidth)
    {
        int contentWidth = Math.Max(1100, tableWidth - 28);
        int[] baseWidths = [230, 135, 170, 120, 135, 140, 170];
        double scale = contentWidth / 1100.0;

        return baseWidths
            .Select(width => (int)Math.Round(width * scale))
            .ToArray();
    }

    private Panel CreateIssueReportTableRow(IssueReportRecord issue, int top, int[] columnWidths, int rowWidth, Action refreshTable)
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
        rowPanel.Controls.Add(CreateTableCell(issue.Title, left, columnWidths[0]));
        left += columnWidths[0];
        rowPanel.Controls.Add(CreateTableCell(issue.ProjectName, left, columnWidths[1]));
        left += columnWidths[1];
        rowPanel.Controls.Add(CreateTableCell(issue.ReportedBy, left, columnWidths[2]));
        left += columnWidths[2];
        rowPanel.Controls.Add(CreatePriorityBadge(issue.Priority, left + 4, 19));
        left += columnWidths[3];
        rowPanel.Controls.Add(CreateIssueStatusBadge(issue.Status, left + 4, 19));
        left += columnWidths[4];
        rowPanel.Controls.Add(CreateTableCell(issue.ReportedDateText, left, columnWidths[5]));
        left += columnWidths[5];

        Button viewButton = CreateRowActionButton("View", PrimaryBlue, left + 6);
        viewButton.Click += (_, _) => ViewIssueReport(issue);
        rowPanel.Controls.Add(viewButton);

        Button deleteButton = CreateRowActionButton("Delete", Red, left + 76);
        deleteButton.Click += (_, _) => DeleteIssueReport(issue, refreshTable);
        rowPanel.Controls.Add(deleteButton);

        AddTableRowSeparator(rowPanel);
        return rowPanel;
    }

    private Label CreateIssueStatusBadge(string status, int left, int top)
    {
        Color backColor = status switch
        {
            "Resolved" => Color.FromArgb(220, 252, 231),
            "In Progress" => Color.FromArgb(219, 234, 254),
            _ => Color.FromArgb(254, 226, 226)
        };

        Color foreColor = status switch
        {
            "Resolved" => Color.FromArgb(22, 101, 52),
            "In Progress" => Color.FromArgb(30, 64, 175),
            _ => Color.FromArgb(185, 28, 28)
        };

        return new Label
        {
            Text = status,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = foreColor,
            BackColor = backColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(status == "In Progress" ? 100 : 82, 30),
            Left = left,
            Top = top
        };
    }

    private List<IssueReportRecord> GetFilteredIssueReports(string searchText, string statusFilter, string priorityFilter, string projectFilter)
    {
        string normalizedSearch = searchText.Trim().ToLowerInvariant();
        string selectedStatus = statusFilter.Replace("Status: ", "");
        string selectedPriority = priorityFilter.Replace("Priority: ", "");
        string selectedProject = projectFilter.Replace("Project: ", "");

        return issueReports
            .Where(issue =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                issue.Title.ToLowerInvariant().Contains(normalizedSearch) ||
                issue.ProjectName.ToLowerInvariant().Contains(normalizedSearch) ||
                issue.ReportedBy.ToLowerInvariant().Contains(normalizedSearch) ||
                issue.Description.ToLowerInvariant().Contains(normalizedSearch))
            .Where(issue => selectedStatus == "All" || issue.Status == selectedStatus)
            .Where(issue => selectedPriority == "All" || issue.Priority == selectedPriority)
            .Where(issue => selectedProject == "All" || issue.ProjectName == selectedProject)
            .ToList();
    }

    private void AddIssueReport(Action refreshTable)
    {
        using IssueReportDialog dialog = new("Report New Issue", projects.Select(project => project.Name), workers.Select(worker => worker.Name));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.IssueReport == null)
        {
            return;
        }

        int nextId = issueReports.Count == 0 ? 1 : issueReports.Max(issue => issue.Id) + 1;
        dialog.IssueReport.Id = nextId;
        issueReports.Add(dialog.IssueReport);
        SaveApplicationData();
        refreshTable();
    }

    private void ViewIssueReport(IssueReportRecord issue)
    {
        MessageBox.Show(
            $"Issue: {issue.Title}\nProject: {issue.ProjectName}\nReported By: {issue.ReportedBy}\nPriority: {issue.Priority}\nStatus: {issue.Status}\nReported Date: {issue.ReportedDateText}\n\nDescription:\n{issue.Description}",
            "Issue Details",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void DeleteIssueReport(IssueReportRecord issue, Action refreshTable)
    {
        DialogResult result = MessageBox.Show(
            $"Delete issue '{issue.Title}'?",
            "Delete Issue",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        issueReports.Remove(issue);
        SaveApplicationData();
        refreshTable();
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartConstructionSiteManagement;

public partial class DashboardForm
{
    private void ShowWorkersContent()
    {
        ResetContentPanel();

        Label titleLabel = CreateModuleTitle("Workers Management");
        contentPanel.Controls.Add(titleLabel);

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
            PlaceholderText = "Search workers...",
            Font = new Font("Segoe UI", 11F),
            Size = new Size(480, 34),
            Left = 16,
            Top = 18
        };
        filterPanel.Controls.Add(searchTextBox);

        ComboBox roleComboBox = CreateFilterComboBox("Role: All", 370);
        roleComboBox.Items.AddRange(["Role: All", "Supervisor", "Mason", "Electrician", "Carpenter", "Plumber", "Safety Officer"]);
        roleComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(roleComboBox);

        ComboBox statusComboBox = CreateFilterComboBox("Status: All", 620);
        statusComboBox.Items.AddRange(["Status: All", "Active", "On Leave", "Inactive"]);
        statusComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(statusComboBox);

        Button addButton = CreateActionButton("Add Worker", PrimaryBlue, 0);
        addButton.Top = 18;
        addButton.Visible = CanAddWorkers();
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

        RoundedPanel workersTable = CreateWorkersTablePanel();
        contentPanel.Controls.Add(workersTable);

        Label showingLabel = new()
        {
            Text = "Showing workers",
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(260, 28),
            Left = 34,
            Top = 730
        };
        contentPanel.Controls.Add(showingLabel);

        void RefreshWorkerStats()
        {
            statPanel.Controls.Clear();
            statPanel.Controls.Add(CreateWorkerStatCard("Total Workers", workers.Count.ToString(), Color.FromArgb(239, 246, 255), Color.FromArgb(47, 128, 237), WorkerStatIcon.Users));
            statPanel.Controls.Add(CreateWorkerStatCard("Active Workers", workers.Count(worker => worker.Status == "Active").ToString(), Color.FromArgb(240, 253, 244), Green, WorkerStatIcon.Check));
            statPanel.Controls.Add(CreateWorkerStatCard("On Leave", workers.Count(worker => worker.Status == "On Leave").ToString(), Color.FromArgb(254, 242, 242), Red, WorkerStatIcon.Clock));
            statPanel.Controls.Add(CreateWorkerStatCard("Inactive", workers.Count(worker => worker.Status == "Inactive").ToString(), Color.FromArgb(255, 251, 235), Orange, WorkerStatIcon.Warning));
        }

        void RefreshWorkersTable()
        {
            List<WorkerRecord> filteredWorkers = GetFilteredWorkers(searchTextBox.Text, roleComboBox.Text, statusComboBox.Text);
            workersTable.Height = GetManagementTableHeight(filteredWorkers.Count);
            PopulateWorkersTable(workersTable, filteredWorkers, RefreshWorkersTable);
            showingLabel.Top = workersTable.Bottom + 20;
            showingLabel.Text = $"Showing {filteredWorkers.Count} of {workers.Count} workers";
            RefreshWorkerStats();
        }

        void ArrangeWorkersPage()
        {
            filterPanel.Width = contentPanel.ClientSize.Width - 68;
            int rightEdge = filterPanel.Width - 16;
            if (addButton.Visible)
            {
                addButton.Left = rightEdge - addButton.Width;
                rightEdge = addButton.Left - 20;
            }

            statusComboBox.Left = rightEdge - statusComboBox.Width;
            roleComboBox.Left = statusComboBox.Left - roleComboBox.Width - 16;
            searchTextBox.Width = Math.Min(520, Math.Max(320, roleComboBox.Left - searchTextBox.Left - 28));
            statPanel.Width = contentPanel.ClientSize.Width - 68;
            List<WorkerRecord> filteredWorkers = GetFilteredWorkers(searchTextBox.Text, roleComboBox.Text, statusComboBox.Text);
            workersTable.SetBounds(34, 315, contentPanel.ClientSize.Width - 68, GetManagementTableHeight(filteredWorkers.Count));
            PopulateWorkersTable(workersTable, filteredWorkers, RefreshWorkersTable);
            showingLabel.Top = workersTable.Bottom + 20;
        }

        searchTextBox.TextChanged += (_, _) => RefreshWorkersTable();
        roleComboBox.SelectedIndexChanged += (_, _) => RefreshWorkersTable();
        statusComboBox.SelectedIndexChanged += (_, _) => RefreshWorkersTable();
        if (CanAddWorkers())
        {
            addButton.Click += (_, _) => AddWorker(RefreshWorkersTable);
        }
        SetContentPanelResizeHandler((_, _) => ArrangeWorkersPage());
        RefreshWorkerStats();
        ArrangeWorkersPage();
        RefreshWorkersTable();
    }

    private Panel CreateWorkerStatCard(string title, string value, Color backColor, Color accentColor, WorkerStatIcon icon)
    {
        RoundedPanel card = new()
        {
            Size = new Size(250, 95),
            Margin = new Padding(0, 0, 20, 0),
            BackColor = backColor,
            BorderRadius = 4,
            BorderColor = BorderColor
        };

        WorkerStatIconPanel iconPanel = new()
        {
            Icon = icon,
            IconColor = accentColor,
            Size = new Size(42, 42),
            Left = 24,
            Top = 26,
            BackColor = backColor
        };
        card.Controls.Add(iconPanel);

        card.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10F),
            ForeColor = TextColor,
            AutoSize = false,
            Size = new Size(150, 24),
            Left = 84,
            Top = 20
        });

        card.Controls.Add(new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 21F),
            ForeColor = accentColor,
            AutoSize = false,
            Size = new Size(150, 44),
            Left = 84,
            Top = 42
        });

        return card;
    }

    private void PopulateWorkersTable(RoundedPanel tablePanel, List<WorkerRecord> filteredWorkers, Action refreshTable)
    {
        tablePanel.SuspendLayout();
        tablePanel.Controls.Clear();

        int[] columnWidths = GetWorkerColumnWidths(tablePanel.Width);
        string[] headers = ["Worker ID", "Full Name", "Role", "Phone", "Email", "Hire Date", "Status", "Actions"];

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
        foreach (WorkerRecord worker in filteredWorkers)
        {
            tablePanel.Controls.Add(CreateWorkerTableRow(worker, rowTop, columnWidths, tablePanel.Width - 2, refreshTable));
            rowTop += 68;
        }

        tablePanel.ResumeLayout();
    }

    private static int[] GetWorkerColumnWidths(int tableWidth)
    {
        int contentWidth = Math.Max(1125, tableWidth - 28);
        int[] baseWidths = [90, 175, 135, 130, 220, 115, 105, 155];
        double scale = contentWidth / 1125.0;

        return baseWidths
            .Select(width => (int)Math.Round(width * scale))
            .ToArray();
    }

    private Panel CreateWorkerTableRow(WorkerRecord worker, int top, int[] columnWidths, int rowWidth, Action refreshTable)
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
        rowPanel.Controls.Add(CreateTableCell(worker.WorkerCode, left, columnWidths[0]));
        left += columnWidths[0];
        rowPanel.Controls.Add(CreateTableCell(worker.Name, left, columnWidths[1]));
        left += columnWidths[1];
        rowPanel.Controls.Add(CreateTableCell(worker.Role, left, columnWidths[2]));
        left += columnWidths[2];
        rowPanel.Controls.Add(CreateTableCell(worker.Phone, left, columnWidths[3]));
        left += columnWidths[3];
        rowPanel.Controls.Add(CreateTableCell(worker.Email, left, columnWidths[4]));
        left += columnWidths[4];
        rowPanel.Controls.Add(CreateTableCell(worker.HireDateText, left, columnWidths[5]));
        left += columnWidths[5];
        rowPanel.Controls.Add(CreateStatusBadge(worker.Status, left + 6, 19));
        left += columnWidths[6];

        if (CanManageWorkers())
        {
            Button editButton = CreateRowActionButton("Edit", PrimaryBlue, left + 6);
            editButton.Click += (_, _) => EditWorker(worker, refreshTable);
            rowPanel.Controls.Add(editButton);
        }

        if (CanDeleteWorkers())
        {
            Button deleteButton = CreateRowActionButton("Delete", Red, left + 76);
            deleteButton.Click += (_, _) => DeleteWorker(worker, refreshTable);
            rowPanel.Controls.Add(deleteButton);
        }

        AddTableRowSeparator(rowPanel);
        return rowPanel;
    }

    private Label CreateStatusBadge(string status, int left, int top)
    {
        Color backColor = status switch
        {
            "Active" => Color.FromArgb(220, 252, 231),
            "On Leave" => Color.FromArgb(254, 243, 199),
            _ => Color.FromArgb(229, 231, 235)
        };

        Color foreColor = status switch
        {
            "Active" => Color.FromArgb(22, 101, 52),
            "On Leave" => Color.FromArgb(146, 64, 14),
            _ => TextColor
        };

        return new Label
        {
            Text = status,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = foreColor,
            BackColor = backColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(status == "On Leave" ? 82 : 72, 30),
            Left = left,
            Top = top
        };
    }

    private List<WorkerRecord> GetFilteredWorkers(string searchText, string roleFilter, string statusFilter)
    {
        string normalizedSearch = searchText.Trim().ToLowerInvariant();
        string selectedRole = roleFilter.Replace("Role: ", "");
        string selectedStatus = statusFilter.Replace("Status: ", "");

        return workers
            .Where(worker =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                worker.WorkerCode.ToLowerInvariant().Contains(normalizedSearch) ||
                worker.Name.ToLowerInvariant().Contains(normalizedSearch) ||
                worker.Role.ToLowerInvariant().Contains(normalizedSearch) ||
                worker.Email.ToLowerInvariant().Contains(normalizedSearch))
            .Where(worker => selectedRole == "All" || worker.Role == selectedRole)
            .Where(worker => selectedStatus == "All" || worker.Status == selectedStatus)
            .ToList();
    }

    private void AddWorker(Action refreshTable)
    {
        using WorkerDialog dialog = new("Add Worker");

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Worker == null)
        {
            return;
        }

        int nextId = workers.Count == 0 ? 1 : workers.Max(worker => worker.Id) + 1;
        dialog.Worker.Id = nextId;
        dialog.Worker.WorkerCode = $"W{nextId:000}";
        workers.Add(dialog.Worker);
        SaveApplicationData();
        refreshTable();
    }

    private void EditWorker(WorkerRecord worker, Action refreshTable)
    {
        using WorkerDialog dialog = new("Edit Worker", worker);

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Worker == null)
        {
            return;
        }

        worker.Name = dialog.Worker.Name;
        worker.Role = dialog.Worker.Role;
        worker.Phone = dialog.Worker.Phone;
        worker.Email = dialog.Worker.Email;
        worker.HireDate = dialog.Worker.HireDate;
        worker.Status = dialog.Worker.Status;
        SaveApplicationData();
        refreshTable();
    }

    private void DeleteWorker(WorkerRecord worker, Action refreshTable)
    {
        DialogResult result = MessageBox.Show(
            $"Delete worker '{worker.Name}'?",
            "Delete Worker",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        workers.Remove(worker);
        SaveApplicationData();
        refreshTable();
    }
}

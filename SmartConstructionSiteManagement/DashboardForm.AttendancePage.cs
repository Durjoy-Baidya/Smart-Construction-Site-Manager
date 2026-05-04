using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartConstructionSiteManagement;

public partial class DashboardForm
{
    private void ShowAttendanceContent()
    {
        contentPanel.Controls.Clear();

        contentPanel.Controls.Add(CreateModuleTitle("Attendance Management"));

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

        bool hasDateFilter = false;
        DateTimePicker dateFilterPicker = new()
        {
            Font = new Font("Segoe UI", 10F),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "'dd/mm/yyyy'",
            Size = new Size(180, 34),
            Left = 520,
            Top = 18,
            Value = attendanceRecords.FirstOrDefault()?.Date ?? DateTime.Today
        };
        filterPanel.Controls.Add(dateFilterPicker);

        ComboBox projectComboBox = CreateFilterComboBox("Project: All", 720);
        projectComboBox.Width = 190;
        projectComboBox.Items.Add("Project: All");
        projectComboBox.Items.AddRange(projects.Select(project => project.Name).Distinct().ToArray());
        projectComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(projectComboBox);

        ComboBox statusComboBox = CreateFilterComboBox("Status: All", 950);
        statusComboBox.Width = 190;
        statusComboBox.Items.AddRange(["Status: All", "Present", "Absent"]);
        statusComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(statusComboBox);

        Button addButton = CreateActionButton("Mark Attendance", PrimaryBlue, 0);
        addButton.Size = new Size(180, 36);
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

        RoundedPanel attendanceTable = CreateWorkersTablePanel();
        contentPanel.Controls.Add(attendanceTable);

        Label showingLabel = new()
        {
            Text = "Showing attendance records",
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(280, 28),
            Left = 34,
            Top = 730
        };
        contentPanel.Controls.Add(showingLabel);

        void RefreshAttendanceStats()
        {
            int presentToday = attendanceRecords.Count(record => record.Status == "Present");
            int absentToday = attendanceRecords.Count(record => record.Status == "Absent");
            decimal attendanceRate = attendanceRecords.Count == 0 ? 0 : presentToday * 100m / attendanceRecords.Count;

            statPanel.Controls.Clear();
            statPanel.Controls.Add(CreatePlainStatCard("Total Workers", attendanceRecords.Count.ToString(), Color.FromArgb(52, 152, 219)));
            statPanel.Controls.Add(CreatePlainStatCard("Present Today", presentToday.ToString(), Green));
            statPanel.Controls.Add(CreatePlainStatCard("Absent Today", absentToday.ToString(), Red));
            statPanel.Controls.Add(CreatePlainStatCard("Attendance Rate", $"{attendanceRate:0.0}%", Orange));
        }

        void RefreshAttendanceTable()
        {
            DateTime? selectedDate = hasDateFilter ? dateFilterPicker.Value.Date : null;
            List<AttendanceRecord> filteredRecords = GetFilteredAttendanceRecords(searchTextBox.Text, projectComboBox.Text, statusComboBox.Text, selectedDate);
            attendanceTable.Height = GetManagementTableHeight(filteredRecords.Count);
            PopulateAttendanceTable(attendanceTable, filteredRecords, RefreshAttendanceTable);
            showingLabel.Top = attendanceTable.Bottom + 20;
            showingLabel.Text = $"Showing {filteredRecords.Count} of {attendanceRecords.Count} attendance records";
            RefreshAttendanceStats();
        }

        void ArrangeAttendancePage()
        {
            filterPanel.Width = contentPanel.ClientSize.Width - 68;
            addButton.Left = filterPanel.Width - addButton.Width - 16;
            statusComboBox.Left = addButton.Left - statusComboBox.Width - 20;
            projectComboBox.Left = statusComboBox.Left - projectComboBox.Width - 16;
            dateFilterPicker.Left = projectComboBox.Left - dateFilterPicker.Width - 16;
            searchTextBox.Width = Math.Min(420, Math.Max(260, dateFilterPicker.Left - searchTextBox.Left - 22));
            statPanel.Width = contentPanel.ClientSize.Width - 68;
            DateTime? selectedDate = hasDateFilter ? dateFilterPicker.Value.Date : null;
            List<AttendanceRecord> filteredRecords = GetFilteredAttendanceRecords(searchTextBox.Text, projectComboBox.Text, statusComboBox.Text, selectedDate);
            attendanceTable.SetBounds(34, 315, contentPanel.ClientSize.Width - 68, GetManagementTableHeight(filteredRecords.Count));
            PopulateAttendanceTable(attendanceTable, filteredRecords, RefreshAttendanceTable);
            showingLabel.Top = attendanceTable.Bottom + 20;
        }

        searchTextBox.TextChanged += (_, _) => RefreshAttendanceTable();
        dateFilterPicker.ValueChanged += (_, _) =>
        {
            hasDateFilter = true;
            dateFilterPicker.CustomFormat = "dd/MM/yyyy";
            RefreshAttendanceTable();
        };
        projectComboBox.SelectedIndexChanged += (_, _) => RefreshAttendanceTable();
        statusComboBox.SelectedIndexChanged += (_, _) => RefreshAttendanceTable();
        addButton.Click += (_, _) => AddAttendanceRecord(RefreshAttendanceTable);
        contentPanel.Resize += (_, _) => ArrangeAttendancePage();
        RefreshAttendanceStats();
        ArrangeAttendancePage();
        RefreshAttendanceTable();
    }

    private void PopulateAttendanceTable(RoundedPanel tablePanel, List<AttendanceRecord> filteredRecords, Action refreshTable)
    {
        tablePanel.SuspendLayout();
        tablePanel.Controls.Clear();

        int[] columnWidths = GetAttendanceColumnWidths(tablePanel.Width);
        string[] headers = ["Worker Name", "ID", "Project", "Check In", "Check Out", "Date", "Status", "Actions"];

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
        foreach (AttendanceRecord record in filteredRecords)
        {
            tablePanel.Controls.Add(CreateAttendanceTableRow(record, rowTop, columnWidths, tablePanel.Width - 2, refreshTable));
            rowTop += 68;
        }

        tablePanel.ResumeLayout();
    }

    private static int[] GetAttendanceColumnWidths(int tableWidth)
    {
        int contentWidth = Math.Max(1125, tableWidth - 28);
        int[] baseWidths = [210, 95, 180, 130, 130, 130, 130, 120];
        double scale = contentWidth / 1125.0;

        return baseWidths
            .Select(width => (int)Math.Round(width * scale))
            .ToArray();
    }

    private Panel CreateAttendanceTableRow(AttendanceRecord record, int top, int[] columnWidths, int rowWidth, Action refreshTable)
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
        rowPanel.Controls.Add(CreateTableCell(record.WorkerName, left, columnWidths[0]));
        left += columnWidths[0];
        rowPanel.Controls.Add(CreateTableCell(record.WorkerCode, left, columnWidths[1]));
        left += columnWidths[1];
        rowPanel.Controls.Add(CreateTableCell(record.ProjectName, left, columnWidths[2]));
        left += columnWidths[2];
        rowPanel.Controls.Add(CreateTableCell(record.CheckIn, left, columnWidths[3]));
        left += columnWidths[3];
        rowPanel.Controls.Add(CreateTableCell(record.CheckOut, left, columnWidths[4]));
        left += columnWidths[4];
        rowPanel.Controls.Add(CreateTableCell(record.DateText, left, columnWidths[5]));
        left += columnWidths[5];
        rowPanel.Controls.Add(CreateAttendanceStatusBadge(record.Status, left + 4, 19));
        left += columnWidths[6];

        Button viewButton = CreateOutlineActionButton("View", PrimaryBlue, left + 6);
        viewButton.Click += (_, _) => ViewAttendanceRecord(record);
        rowPanel.Controls.Add(viewButton);

        AddTableRowSeparator(rowPanel);
        return rowPanel;
    }

    private static Label CreateAttendanceStatusBadge(string status, int left, int top)
    {
        Color backColor = status switch
        {
            "Present" => Color.FromArgb(220, 252, 231),
            "Late" => Color.FromArgb(255, 251, 235),
            _ => Color.FromArgb(254, 226, 226)
        };

        Color foreColor = status switch
        {
            "Present" => Color.FromArgb(22, 101, 52),
            "Late" => Color.FromArgb(146, 64, 14),
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
            Size = new Size(82, 30),
            Left = left,
            Top = top
        };
    }

    private List<AttendanceRecord> GetFilteredAttendanceRecords(string searchText, string projectFilter, string statusFilter, DateTime? dateFilter)
    {
        string normalizedSearch = searchText.Trim().ToLowerInvariant();
        string selectedProject = projectFilter.Replace("Project: ", "");
        string selectedStatus = statusFilter.Replace("Status: ", "");

        return attendanceRecords
            .Where(record =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                record.WorkerCode.ToLowerInvariant().Contains(normalizedSearch) ||
                record.WorkerName.ToLowerInvariant().Contains(normalizedSearch) ||
                record.Role.ToLowerInvariant().Contains(normalizedSearch) ||
                record.ProjectName.ToLowerInvariant().Contains(normalizedSearch))
            .Where(record => !dateFilter.HasValue || record.Date.Date == dateFilter.Value.Date)
            .Where(record => selectedProject == "All" || record.ProjectName == selectedProject)
            .Where(record => selectedStatus == "All" || record.Status == selectedStatus)
            .ToList();
    }

    private void AddAttendanceRecord(Action refreshTable)
    {
        using AttendanceDialog dialog = new("Add Attendance Record", null, workers, projects.Select(project => project.Name));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Attendance == null)
        {
            return;
        }

        int nextId = attendanceRecords.Count == 0 ? 1 : attendanceRecords.Max(record => record.Id) + 1;
        dialog.Attendance.Id = nextId;
        attendanceRecords.Add(dialog.Attendance);
        SaveApplicationData();
        refreshTable();
    }

    private void ViewAttendanceRecord(AttendanceRecord record)
    {
        MessageBox.Show(
            $"Worker: {record.WorkerName}\nID: {record.WorkerCode}\nProject: {record.ProjectName}\nDate: {record.DateText}\nCheck In: {record.CheckIn}\nCheck Out: {record.CheckOut}\nStatus: {record.Status}",
            "Attendance Details",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void EditAttendanceRecord(AttendanceRecord record, Action refreshTable)
    {
        using AttendanceDialog dialog = new("Edit Attendance Record", record, workers, projects.Select(project => project.Name));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Attendance == null)
        {
            return;
        }

        record.WorkerName = dialog.Attendance.WorkerName;
        record.WorkerCode = dialog.Attendance.WorkerCode;
        record.Role = dialog.Attendance.Role;
        record.ProjectName = dialog.Attendance.ProjectName;
        record.Date = dialog.Attendance.Date;
        record.CheckIn = dialog.Attendance.CheckIn;
        record.CheckOut = dialog.Attendance.CheckOut;
        record.Status = dialog.Attendance.Status;
        SaveApplicationData();
        refreshTable();
    }

    private void DeleteAttendanceRecord(AttendanceRecord record, Action refreshTable)
    {
        DialogResult result = MessageBox.Show(
            $"Delete attendance record for '{record.WorkerName}'?",
            "Delete Attendance Record",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        attendanceRecords.Remove(record);
        SaveApplicationData();
        refreshTable();
    }
}

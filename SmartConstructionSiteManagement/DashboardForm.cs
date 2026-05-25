using System.ComponentModel;

namespace SmartConstructionSiteManagement;

public partial class DashboardForm : Form
{
    private static readonly Color PageBackColor = Color.FromArgb(248, 250, 252);
    private static readonly Color BorderColor = Color.FromArgb(222, 226, 232);
    private static readonly Color TextColor = Color.FromArgb(25, 41, 65);
    private static readonly Color MutedTextColor = Color.FromArgb(105, 116, 133);
    private static readonly Color PrimaryBlue = Color.FromArgb(37, 99, 235);
    private static readonly Color Green = Color.FromArgb(52, 199, 123);
    private static readonly Color Orange = Color.FromArgb(249, 159, 10);
    private static readonly Color Red = Color.FromArgb(239, 68, 68);
    private static readonly Color TableRowLineColor = Color.FromArgb(210, 215, 222);

    private readonly IAppDataStore appDataStore;
    private readonly UserAccount currentUser;
    private Panel contentPanel = null!;
    private SidebarNavButton selectedButton = null!;
    private readonly BindingList<WorkerRecord> workers = new(SeedData.CreateDefaultWorkers());
    private readonly BindingList<ProjectRecord> projects = new(SeedData.CreateDefaultProjects());
    private readonly BindingList<TaskRecord> tasks = new(SeedData.CreateDefaultTasks());
    private readonly BindingList<AttendanceRecord> attendanceRecords = new(SeedData.CreateDefaultAttendanceRecords());
    private readonly BindingList<MaterialRecord> materials = new(SeedData.CreateDefaultMaterials());
    private readonly BindingList<IssueReportRecord> issueReports = new(SeedData.CreateDefaultIssueReports());
    private const int SidebarWidth = 235;
    private const int HeaderHeight = 78;
    private const int MinimumManagementRows = 20;
    private const int ManagementTableHeaderHeight = 55;
    private const int ManagementTableRowHeight = 68;

    public DashboardForm()
        : this(new SqliteAppDataStore(), new UserAccount
        {
            Id = 2,
            Name = "John Anderson",
            Email = "manager@site.com",
            Role = UserRole.SiteManager,
            ContactNumber = "+1 555-0002"
        })
    {
    }

    public DashboardForm(IAppDataStore appDataStore)
        : this(appDataStore, new UserAccount
        {
            Id = 2,
            Name = "John Anderson",
            Email = "manager@site.com",
            Role = UserRole.SiteManager,
            ContactNumber = "+1 555-0002"
        })
    {
    }

    public DashboardForm(IAppDataStore appDataStore, UserAccount currentUser)
    {
        this.appDataStore = appDataStore;
        this.currentUser = currentUser;
        LoadApplicationData();
        BuildDashboardUI();
    }

    private void LoadApplicationData()
    {
        ApplicationData? savedData = appDataStore.Load();

        if (savedData == null || !SeedData.HasManagementData(savedData))
        {
            SaveApplicationData();
            return;
        }

        ReplaceListContents(workers, savedData.Workers);
        ReplaceListContents(projects, savedData.Projects);
        ReplaceListContents(tasks, savedData.Tasks);
        ReplaceListContents(attendanceRecords, savedData.AttendanceRecords);
        ReplaceListContents(materials, savedData.Materials);
        ReplaceListContents(issueReports, savedData.IssueReports);
    }

    private void SaveApplicationData()
    {
        appDataStore.Save(new ApplicationData
        {
            Users = (appDataStore.Load()?.Users ?? []).ToList(),
            Workers = workers.ToList(),
            Projects = projects.ToList(),
            Tasks = tasks.ToList(),
            AttendanceRecords = attendanceRecords.ToList(),
            Materials = materials.ToList(),
            IssueReports = issueReports.ToList()
        });
    }

    private static void ReplaceListContents<T>(BindingList<T> target, IEnumerable<T> source)
    {
        target.Clear();

        foreach (T item in source)
        {
            target.Add(item);
        }
    }

    private void BuildDashboardUI()
    {
        SuspendLayout();

        Text = "Smart Construction Site Management - Dashboard";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1365, 820);
        MinimumSize = new Size(1100, 700);
        BackColor = PageBackColor;
        Font = new Font("Segoe UI", 10F);

        Panel mainPanel = new()
        {
            Location = new Point(SidebarWidth, 0),
            Size = new Size(ClientSize.Width - SidebarWidth, ClientSize.Height),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = PageBackColor
        };
        Controls.Add(mainPanel);

        Panel sidebarPanel = BuildSidebar();
        Controls.Add(sidebarPanel);
        sidebarPanel.BringToFront();

        Panel headerPanel = BuildHeader();
        headerPanel.Location = new Point(0, 0);
        headerPanel.Size = new Size(mainPanel.Width, HeaderHeight);
        headerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        mainPanel.Controls.Add(headerPanel);

        contentPanel = new Panel
        {
            Location = new Point(0, HeaderHeight),
            Size = new Size(mainPanel.Width, mainPanel.Height - HeaderHeight),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = PageBackColor,
            AutoScroll = true
        };
        mainPanel.Controls.Add(contentPanel);
        headerPanel.BringToFront();

        ShowDashboardContent();

        ResumeLayout();
    }

    private Panel BuildSidebar()
    {
        Panel sidebarPanel = new()
        {
            Width = 235,
            Height = ClientSize.Height,
            Location = new Point(0, 0),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
            BackColor = Color.White
        };
        sidebarPanel.Paint += (_, e) =>
        {
            using Pen borderPen = new(BorderColor, 1);
            e.Graphics.DrawLine(borderPen, sidebarPanel.Width - 1, 0, sidebarPanel.Width - 1, sidebarPanel.Height);
        };

        (string Text, SidebarIconKind Icon)[] menuItems = GetMenuItemsForCurrentUser();

        int top = 72;
        foreach ((string text, SidebarIconKind icon) in menuItems)
        {
            SidebarNavButton button = CreateSidebarButton(text, icon, top);
            button.Click += (_, _) => SelectMenu(button);
            sidebarPanel.Controls.Add(button);
            top += 54;

            if (selectedButton == null!)
            {
                selectedButton = button;
                selectedButton.BackColor = PrimaryBlue;
                selectedButton.ForeColor = Color.White;
            }
        }

        SidebarNavButton logoutButton = CreateSidebarButton("Logout", SidebarIconKind.Logout, 0);
        logoutButton.Top = sidebarPanel.Height - 78;
        logoutButton.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        logoutButton.Click += (_, _) => Close();
        sidebarPanel.Controls.Add(logoutButton);

        return sidebarPanel;
    }

    private (string Text, SidebarIconKind Icon)[] GetMenuItemsForCurrentUser()
    {
        (string Text, SidebarIconKind Icon)[] allMenuItems =
        [
            ("Dashboard", SidebarIconKind.Home),
            ("Workers", SidebarIconKind.Users),
            ("Projects", SidebarIconKind.Projects),
            ("Tasks", SidebarIconKind.Tasks),
            ("Attendance", SidebarIconKind.Calendar),
            ("Materials", SidebarIconKind.Materials),
            ("Issue Reports", SidebarIconKind.Warning)
        ];

        if (currentUser.Role == UserRole.Worker)
        {
            return allMenuItems
                .Where(item => item.Text is "Dashboard" or "Tasks" or "Issue Reports")
                .ToArray();
        }

        return allMenuItems;
    }

    private bool CanAddWorkers() => currentUser.Role == UserRole.Admin;

    private bool CanManageWorkers() => currentUser.Role is UserRole.Admin or UserRole.SiteManager;

    private bool CanDeleteWorkers() => currentUser.Role == UserRole.Admin;

    private bool CanManageProjects() => currentUser.Role is UserRole.Admin or UserRole.SiteManager;

    private bool CanManageTasks() => currentUser.Role is UserRole.Admin or UserRole.SiteManager;

    private bool CanManageAttendance() => currentUser.Role is UserRole.Admin or UserRole.SiteManager;

    private bool CanManageMaterials() => currentUser.Role is UserRole.Admin or UserRole.SiteManager;

    private bool CanReportIssues() => currentUser.Role is UserRole.Admin or UserRole.SiteManager or UserRole.Worker;

    private bool CanDeleteIssues() => currentUser.Role is UserRole.Admin or UserRole.SiteManager;

    private Panel BuildHeader()
    {
        Panel headerPanel = new()
        {
            Height = 78,
            BackColor = Color.White
        };
        headerPanel.Paint += (_, e) =>
        {
            using Pen borderPen = new(BorderColor, 1);
            e.Graphics.DrawLine(borderPen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
        };

        Label titleLabel = new()
        {
            Text = "Smart Construction Site Management",
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = false,
            Size = new Size(680, 40),
            Left = 34,
            Top = 20
        };
        headerPanel.Controls.Add(titleLabel);

        Label nameLabel = new()
        {
            Text = currentUser.Name,
            Font = new Font("Segoe UI", 12F),
            ForeColor = TextColor,
            TextAlign = ContentAlignment.MiddleRight,
            AutoSize = false,
            Size = new Size(160, 25),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Left = 860,
            Top = 14
        };
        headerPanel.Controls.Add(nameLabel);

        Label roleLabel = new()
        {
            Text = GetRoleDisplayName(currentUser.Role),
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            TextAlign = ContentAlignment.MiddleRight,
            AutoSize = false,
            Size = new Size(160, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Left = 860,
            Top = 40
        };
        headerPanel.Controls.Add(roleLabel);

        Label avatarLabel = new()
        {
            Text = GetInitials(currentUser.Name),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = PrimaryBlue,
            AutoSize = false,
            Size = new Size(42, 42),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Left = 1032,
            Top = 18
        };
        headerPanel.Controls.Add(avatarLabel);

        headerPanel.Resize += (_, _) =>
        {
            avatarLabel.Left = headerPanel.Width - 64;
            nameLabel.Left = avatarLabel.Left - 172;
            roleLabel.Left = avatarLabel.Left - 172;
        };

        return headerPanel;
    }

    private static string GetRoleDisplayName(UserRole role)
    {
        return role switch
        {
            UserRole.SiteManager => "Site Manager",
            _ => role.ToString()
        };
    }

    private static string GetInitials(string name)
    {
        string[] nameParts = name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (nameParts.Length == 0)
        {
            return "U";
        }

        return string.Concat(nameParts.Take(2).Select(part => part[0])).ToUpperInvariant();
    }

    private SidebarNavButton CreateSidebarButton(string text, SidebarIconKind icon, int top)
    {
        SidebarNavButton button = new()
        {
            Text = text,
            IconKind = icon,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            Size = new Size(235, 54),
            Left = 0,
            Top = top,
            BackColor = Color.White,
            ForeColor = TextColor,
            Cursor = Cursors.Hand
        };

        return button;
    }

    private void SelectMenu(SidebarNavButton button)
    {
        selectedButton.BackColor = Color.White;
        selectedButton.ForeColor = TextColor;

        selectedButton = button;
        selectedButton.BackColor = PrimaryBlue;
        selectedButton.ForeColor = Color.White;

        if (button.Text == "Dashboard")
        {
            ShowDashboardContent();
        }
        else if (button.Text == "Workers")
        {
            ShowWorkersContent();
        }
        else if (button.Text == "Projects")
        {
            ShowProjectsContent();
        }
        else if (button.Text == "Tasks")
        {
            ShowTasksContent();
        }
        else if (button.Text == "Attendance")
        {
            ShowAttendanceContent();
        }
        else if (button.Text == "Materials")
        {
            ShowMaterialsContent();
        }
        else if (button.Text == "Issue Reports")
        {
            ShowIssueReportsContent();
        }
        else
        {
            ShowModulePlaceholder(button.Text);
        }
    }

    private void ShowModulePlaceholder(string moduleName)
    {
        contentPanel.Controls.Clear();
        contentPanel.Controls.Add(CreateModuleTitle(moduleName));

        RoundedPanel panel = new()
        {
            Location = new Point(34, 98),
            Size = new Size(620, 150),
            BackColor = Color.White,
            BorderRadius = 4,
            BorderColor = BorderColor
        };
        contentPanel.Controls.Add(panel);

        panel.Controls.Add(new Label
        {
            Text = $"{moduleName} module will be built using the same table and form pattern.",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = false,
            Size = new Size(560, 32),
            Left = 24,
            Top = 34
        });

        panel.Controls.Add(new Label
        {
            Text = "We will add this after Worker Management is complete.",
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(560, 28),
            Left = 24,
            Top = 74
        });
    }

    private Label CreateModuleTitle(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = false,
            Size = new Size(560, 48),
            Left = 34,
            Top = 28
        };
    }

    private Button CreateActionButton(string text, Color backColor, int left)
    {
        Button button = new()
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Size = new Size(text.StartsWith("Add ") ? 120 : 90, 36),
            Left = left,
            Top = 84,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;

        return button;
    }

    private static ComboBox CreateFilterComboBox(string text, int left)
    {
        return new ComboBox
        {
            Text = text,
            Font = new Font("Segoe UI", 10F),
            Size = new Size(230, 34),
            Left = left,
            Top = 18,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
    }

    private Panel CreatePlainStatCard(string title, string value, Color valueColor)
    {
        RoundedPanel card = new()
        {
            Size = new Size(250, 95),
            Margin = new Padding(0, 0, 20, 0),
            BackColor = Color.White,
            BorderRadius = 4,
            BorderColor = BorderColor
        };

        card.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10F),
            ForeColor = TextColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(220, 24),
            Left = 15,
            Top = 18
        });

        card.Controls.Add(new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 21F),
            ForeColor = valueColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(220, 44),
            Left = 15,
            Top = 44
        });

        return card;
    }

    private RoundedPanel CreateWorkersTablePanel()
    {
        return new RoundedPanel
        {
            BackColor = Color.White,
            BorderRadius = 4,
            BorderColor = BorderColor
        };
    }

    private static int GetManagementTableHeight(int rowCount)
    {
        return ManagementTableHeaderHeight + (Math.Max(MinimumManagementRows, rowCount) * ManagementTableRowHeight) + 2;
    }

    private Label CreateTableHeaderLabel(string text, int left, int width)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Size = new Size(width, 54),
            Left = left,
            Top = 0
        };
    }

    private Label CreateTableCell(string text, int left, int width)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10F),
            ForeColor = TextColor,
            BackColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Size = new Size(width, 68),
            Left = left,
            Top = 0
        };
    }

    private static void AddTableRowSeparator(Control rowControl)
    {
        Panel separator = new()
        {
            BackColor = TableRowLineColor,
            Dock = DockStyle.Bottom,
            Height = 1
        };
        rowControl.Controls.Add(separator);
        separator.BringToFront();
    }

    private Button CreateRowActionButton(string text, Color backColor, int left)
    {
        Button button = new()
        {
            Text = text,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = backColor,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(text == "Delete" ? 72 : 58, 36),
            Left = left,
            Top = 16,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;

        return button;
    }

    private static Button CreateOutlineActionButton(string text, Color borderColor, int left)
    {
        Button button = new()
        {
            Text = text,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = borderColor,
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(70, 36),
            Left = left,
            Top = 16,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.BorderSize = 1;

        return button;
    }

    private DataGridView CreateWorkersGrid()
    {
        DataGridView grid = new()
        {
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 52,
            RowTemplate = { Height = 68 }
        };

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = TextColor;
        grid.GridColor = Color.FromArgb(226, 232, 240);

        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Worker ID", DataPropertyName = "WorkerCode", FillWeight = 75 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Full Name", DataPropertyName = "Name", FillWeight = 135 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Role", DataPropertyName = "Role", FillWeight = 115 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phone", DataPropertyName = "Phone", FillWeight = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email", FillWeight = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hire Date", DataPropertyName = "HireDateText", FillWeight = 95 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status", FillWeight = 85 });
        grid.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "Actions",
            Name = "EditAction",
            Text = "Edit",
            UseColumnTextForButtonValue = true,
            FillWeight = 65,
            FlatStyle = FlatStyle.Flat
        });
        grid.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "",
            Name = "DeleteAction",
            Text = "Delete",
            UseColumnTextForButtonValue = true,
            FillWeight = 75,
            FlatStyle = FlatStyle.Flat
        });

        return grid;
    }

    private void ApplyWorkerFilters(DataGridView grid, string searchText, string roleFilter, string statusFilter)
    {
        string normalizedSearch = searchText.Trim().ToLowerInvariant();
        string selectedRole = roleFilter.Replace("Role: ", "");
        string selectedStatus = statusFilter.Replace("Status: ", "");

        List<WorkerRecord> filteredWorkers = workers
            .Where(worker =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                worker.WorkerCode.ToLowerInvariant().Contains(normalizedSearch) ||
                worker.Name.ToLowerInvariant().Contains(normalizedSearch) ||
                worker.Role.ToLowerInvariant().Contains(normalizedSearch) ||
                worker.Email.ToLowerInvariant().Contains(normalizedSearch))
            .Where(worker => selectedRole == "All" || worker.Role == selectedRole)
            .Where(worker => selectedStatus == "All" || worker.Status == selectedStatus)
            .ToList();

        grid.DataSource = new BindingList<WorkerRecord>(filteredWorkers);
    }

    private void AddWorker(DataGridView grid)
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
        grid.DataSource = workers;
    }

    private void EditSelectedWorker(DataGridView grid)
    {
        WorkerRecord? selectedWorker = GetSelectedWorker(grid);

        if (selectedWorker == null)
        {
            MessageBox.Show("Please select a worker to edit.", "Edit Worker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using WorkerDialog dialog = new("Edit Worker", selectedWorker);

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Worker == null)
        {
            return;
        }

        selectedWorker.Name = dialog.Worker.Name;
        selectedWorker.Role = dialog.Worker.Role;
        selectedWorker.Phone = dialog.Worker.Phone;
        selectedWorker.Email = dialog.Worker.Email;
        selectedWorker.HireDate = dialog.Worker.HireDate;
        selectedWorker.Status = dialog.Worker.Status;
        grid.Refresh();
    }

    private void DeleteSelectedWorker(DataGridView grid)
    {
        WorkerRecord? selectedWorker = GetSelectedWorker(grid);

        if (selectedWorker == null)
        {
            MessageBox.Show("Please select a worker to delete.", "Delete Worker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult result = MessageBox.Show(
            $"Delete worker '{selectedWorker.Name}'?",
            "Delete Worker",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            workers.Remove(selectedWorker);
            grid.DataSource = workers;
        }
    }

    private static WorkerRecord? GetSelectedWorker(DataGridView grid)
    {
        if (grid.CurrentRow?.DataBoundItem is WorkerRecord worker)
        {
            return worker;
        }

        return null;
    }

    private void HandleWorkerGridAction(DataGridView grid, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        WorkerRecord? worker = grid.Rows[e.RowIndex].DataBoundItem as WorkerRecord;
        if (worker == null)
        {
            return;
        }

        string columnName = grid.Columns[e.ColumnIndex].Name;

        if (columnName == "EditAction")
        {
            using WorkerDialog dialog = new("Edit Worker", worker);
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Worker != null)
            {
                worker.Name = dialog.Worker.Name;
                worker.Role = dialog.Worker.Role;
                worker.Phone = dialog.Worker.Phone;
                worker.Email = dialog.Worker.Email;
                worker.HireDate = dialog.Worker.HireDate;
                worker.Status = dialog.Worker.Status;
                grid.Refresh();
            }
        }

        if (columnName == "DeleteAction")
        {
            DialogResult result = MessageBox.Show(
                $"Delete worker '{worker.Name}'?",
                "Delete Worker",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                workers.Remove(worker);
                grid.DataSource = workers;
            }
        }
    }

    private void FormatWorkerGridCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0)
        {
            return;
        }

        string columnName = grid.Columns[e.ColumnIndex].Name;

        if (grid.Columns[e.ColumnIndex].DataPropertyName == "Status" && e.Value is string status)
        {
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            if (status == "Active")
            {
                e.CellStyle.BackColor = Color.FromArgb(220, 252, 231);
                e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52);
            }
            else if (status == "On Leave")
            {
                e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                e.CellStyle.ForeColor = Color.FromArgb(146, 64, 14);
            }
            else
            {
                e.CellStyle.BackColor = Color.FromArgb(229, 231, 235);
                e.CellStyle.ForeColor = TextColor;
            }
        }

        if (columnName == "EditAction")
        {
            e.CellStyle.BackColor = PrimaryBlue;
            e.CellStyle.ForeColor = Color.White;
            e.CellStyle.SelectionBackColor = PrimaryBlue;
            e.CellStyle.SelectionForeColor = Color.White;
        }

        if (columnName == "DeleteAction")
        {
            e.CellStyle.BackColor = Red;
            e.CellStyle.ForeColor = Color.White;
            e.CellStyle.SelectionBackColor = Red;
            e.CellStyle.SelectionForeColor = Color.White;
        }
    }

    private Control CreateActivityItem(string title, string time, Color dotColor, int top)
    {
        Panel itemPanel = new()
        {
            Left = 24,
            Top = top,
            Width = 460,
            Height = 72,
            BackColor = Color.White,
            Tag = "activity-row"
        };

        DotPanel dot = new()
        {
            DotColor = dotColor,
            Size = new Size(14, 14),
            Left = 0,
            Top = 10
        };
        itemPanel.Controls.Add(dot);

        itemPanel.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = false,
            Size = new Size(390, 28),
            Left = 30,
            Top = 0
        });

        itemPanel.Controls.Add(new Label
        {
            Text = time,
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(260, 24),
            Left = 30,
            Top = 34
        });

        return itemPanel;
    }

    private Control CreateStatBar(string title, string percentText, Color barColor, int top)
    {
        Panel statPanel = new()
        {
            Left = 24,
            Top = top,
            Width = 560,
            Height = 65,
            Tag = "stat-row",
            BackColor = Color.White
        };

        Label titleLabel = new()
        {
            Text = title,
            Font = new Font("Segoe UI", 11F),
            ForeColor = TextColor,
            AutoSize = false,
            Size = new Size(250, 26),
            Left = 0,
            Top = 0
        };
        statPanel.Controls.Add(titleLabel);

        Label percentLabel = new()
        {
            Text = percentText,
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            TextAlign = ContentAlignment.MiddleRight,
            AutoSize = false,
            Size = new Size(70, 26),
            Top = 0,
            Tag = "percent"
        };
        statPanel.Controls.Add(percentLabel);

        ProgressLine progressLine = new()
        {
            ProgressColor = barColor,
            Progress = int.Parse(percentText.TrimEnd('%')),
            Size = new Size(540, 12),
            Left = 0,
            Top = 36
        };
        statPanel.Controls.Add(progressLine);

        return statPanel;
    }
}

public class DotPanel : Panel
{
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color DotColor { get; set; } = Color.Blue;

    public DotPanel()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using SolidBrush brush = new(DotColor);
        e.Graphics.FillEllipse(brush, 0, 0, Width - 1, Height - 1);
    }
}

public enum WorkerStatIcon
{
    Users,
    Check,
    Clock,
    Warning
}

public class WorkerStatIconPanel : Panel
{
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public WorkerStatIcon Icon { get; set; }

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color IconColor { get; set; } = Color.Blue;

    public WorkerStatIconPanel()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);

        using Pen pen = new(IconColor, 2.2F)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };

        Rectangle bounds = new(5, 5, Width - 10, Height - 10);

        switch (Icon)
        {
            case WorkerStatIcon.Users:
                e.Graphics.DrawEllipse(pen, bounds.Left + 3, bounds.Top + 8, 10, 10);
                e.Graphics.DrawEllipse(pen, bounds.Left + 18, bounds.Top + 8, 10, 10);
                e.Graphics.DrawArc(pen, bounds.Left, bounds.Top + 18, 18, 13, 200, 140);
                e.Graphics.DrawArc(pen, bounds.Left + 15, bounds.Top + 18, 18, 13, 200, 140);
                break;
            case WorkerStatIcon.Check:
                e.Graphics.DrawEllipse(pen, bounds);
                e.Graphics.DrawLine(pen, bounds.Left + 9, bounds.Top + 17, bounds.Left + 15, bounds.Top + 23);
                e.Graphics.DrawLine(pen, bounds.Left + 15, bounds.Top + 23, bounds.Right - 8, bounds.Top + 11);
                break;
            case WorkerStatIcon.Clock:
                e.Graphics.DrawEllipse(pen, bounds);
                e.Graphics.DrawLine(pen, bounds.Left + bounds.Width / 2, bounds.Top + 8, bounds.Left + bounds.Width / 2, bounds.Top + 18);
                e.Graphics.DrawLine(pen, bounds.Left + bounds.Width / 2, bounds.Top + 18, bounds.Left + bounds.Width / 2 + 8, bounds.Top + 22);
                break;
            case WorkerStatIcon.Warning:
                Point[] warning =
                [
                    new(bounds.Left + bounds.Width / 2, bounds.Top + 2),
                    new(bounds.Right - 2, bounds.Bottom - 2),
                    new(bounds.Left + 2, bounds.Bottom - 2)
                ];
                e.Graphics.DrawPolygon(pen, warning);
                e.Graphics.DrawLine(pen, bounds.Left + bounds.Width / 2, bounds.Top + 12, bounds.Left + bounds.Width / 2, bounds.Top + 22);
                e.Graphics.DrawLine(pen, bounds.Left + bounds.Width / 2, bounds.Bottom - 8, bounds.Left + bounds.Width / 2, bounds.Bottom - 7);
                break;
        }
    }
}

public enum SidebarIconKind
{
    Home,
    Users,
    Projects,
    Tasks,
    Calendar,
    Materials,
    Warning,
    Logout
}

public class SidebarNavButton : Button
{
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public SidebarIconKind IconKind { get; set; }

    public SidebarNavButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        TextAlign = ContentAlignment.MiddleLeft;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        pevent.Graphics.Clear(BackColor);

        Color iconColor = ForeColor;
        using Pen iconPen = new(iconColor, 1.8F)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };
        using SolidBrush textBrush = new(ForeColor);

        Rectangle iconBounds = new(34, 18, 22, 22);
        DrawIcon(pevent.Graphics, iconPen, iconBounds);

        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            new Rectangle(70, 0, Width - 74, Height),
            ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    private void DrawIcon(Graphics graphics, Pen pen, Rectangle bounds)
    {
        switch (IconKind)
        {
            case SidebarIconKind.Home:
                Point roofTop = new(bounds.Left + bounds.Width / 2, bounds.Top + 2);
                graphics.DrawLine(pen, bounds.Left + 2, bounds.Top + 11, roofTop.X, roofTop.Y);
                graphics.DrawLine(pen, roofTop.X, roofTop.Y, bounds.Right - 2, bounds.Top + 11);
                graphics.DrawRectangle(pen, bounds.Left + 5, bounds.Top + 10, bounds.Width - 10, bounds.Height - 8);
                break;
            case SidebarIconKind.Users:
                graphics.DrawEllipse(pen, bounds.Left + 3, bounds.Top + 3, 8, 8);
                graphics.DrawEllipse(pen, bounds.Left + 12, bounds.Top + 5, 7, 7);
                graphics.DrawArc(pen, bounds.Left + 1, bounds.Top + 12, 13, 11, 200, 140);
                graphics.DrawArc(pen, bounds.Left + 10, bounds.Top + 13, 11, 9, 200, 140);
                break;
            case SidebarIconKind.Projects:
                graphics.DrawRectangle(pen, bounds.Left + 3, bounds.Top + 5, bounds.Width - 6, bounds.Height - 8);
                graphics.DrawLine(pen, bounds.Left + 7, bounds.Top + 5, bounds.Left + 7, bounds.Top + 1);
                graphics.DrawLine(pen, bounds.Left + 7, bounds.Top + 1, bounds.Left + 13, bounds.Top + 1);
                graphics.DrawLine(pen, bounds.Left + 13, bounds.Top + 1, bounds.Left + 15, bounds.Top + 5);
                graphics.DrawLine(pen, bounds.Left + 7, bounds.Top + 10, bounds.Right - 7, bounds.Top + 10);
                break;
            case SidebarIconKind.Tasks:
                graphics.DrawRectangle(pen, bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 6);
                graphics.DrawRectangle(pen, bounds.Left + 8, bounds.Top + 1, bounds.Width - 16, 5);
                graphics.DrawLine(pen, bounds.Left + 8, bounds.Top + 11, bounds.Right - 7, bounds.Top + 11);
                graphics.DrawLine(pen, bounds.Left + 8, bounds.Top + 16, bounds.Right - 7, bounds.Top + 16);
                break;
            case SidebarIconKind.Calendar:
                graphics.DrawRectangle(pen, bounds.Left + 3, bounds.Top + 5, bounds.Width - 6, bounds.Height - 7);
                graphics.DrawLine(pen, bounds.Left + 3, bounds.Top + 10, bounds.Right - 3, bounds.Top + 10);
                graphics.DrawLine(pen, bounds.Left + 8, bounds.Top + 2, bounds.Left + 8, bounds.Top + 7);
                graphics.DrawLine(pen, bounds.Right - 8, bounds.Top + 2, bounds.Right - 8, bounds.Top + 7);
                break;
            case SidebarIconKind.Materials:
                Point[] cube =
                [
                    new(bounds.Left + 11, bounds.Top + 2),
                    new(bounds.Right - 3, bounds.Top + 8),
                    new(bounds.Right - 3, bounds.Bottom - 7),
                    new(bounds.Left + 11, bounds.Bottom - 1),
                    new(bounds.Left + 3, bounds.Bottom - 7),
                    new(bounds.Left + 3, bounds.Top + 8)
                ];
                graphics.DrawPolygon(pen, cube);
                graphics.DrawLine(pen, bounds.Left + 11, bounds.Top + 2, bounds.Left + 11, bounds.Bottom - 1);
                graphics.DrawLine(pen, bounds.Left + 3, bounds.Top + 8, bounds.Left + 11, bounds.Top + 14);
                graphics.DrawLine(pen, bounds.Right - 3, bounds.Top + 8, bounds.Left + 11, bounds.Top + 14);
                break;
            case SidebarIconKind.Warning:
                Point[] warning =
                [
                    new(bounds.Left + bounds.Width / 2, bounds.Top + 2),
                    new(bounds.Right - 2, bounds.Bottom - 2),
                    new(bounds.Left + 2, bounds.Bottom - 2)
                ];
                graphics.DrawPolygon(pen, warning);
                graphics.DrawLine(pen, bounds.Left + bounds.Width / 2, bounds.Top + 8, bounds.Left + bounds.Width / 2, bounds.Top + 14);
                graphics.DrawLine(pen, bounds.Left + bounds.Width / 2, bounds.Bottom - 6, bounds.Left + bounds.Width / 2, bounds.Bottom - 5);
                break;
            case SidebarIconKind.Logout:
                graphics.DrawLine(pen, bounds.Left + 4, bounds.Top + 4, bounds.Left + 4, bounds.Bottom - 4);
                graphics.DrawLine(pen, bounds.Left + 4, bounds.Top + 4, bounds.Left + 12, bounds.Top + 4);
                graphics.DrawLine(pen, bounds.Left + 4, bounds.Bottom - 4, bounds.Left + 12, bounds.Bottom - 4);
                graphics.DrawLine(pen, bounds.Left + 10, bounds.Top + 11, bounds.Right - 3, bounds.Top + 11);
                graphics.DrawLine(pen, bounds.Right - 8, bounds.Top + 6, bounds.Right - 3, bounds.Top + 11);
                graphics.DrawLine(pen, bounds.Right - 8, bounds.Top + 16, bounds.Right - 3, bounds.Top + 11);
                break;
        }
    }
}

public class ProgressLine : Panel
{
    private int progress;
    private Color progressColor = Color.Blue;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Progress
    {
        get => progress;
        set
        {
            progress = Math.Clamp(value, 0, 100);
            Invalidate();
        }
    }

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color ProgressColor
    {
        get => progressColor;
        set
        {
            progressColor = value;
            Invalidate();
        }
    }

    public ProgressLine()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);

        Rectangle track = new(0, 2, Width - 1, 8);
        Rectangle fill = new(0, 2, Math.Max(1, (Width - 1) * Progress / 100), 8);

        using SolidBrush trackBrush = new(Color.FromArgb(229, 233, 238));
        using SolidBrush fillBrush = new(ProgressColor);
        e.Graphics.FillRectangle(trackBrush, track);
        e.Graphics.FillRectangle(fillBrush, fill);
    }
}

public class MaterialRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalValue => Quantity * UnitPrice;
    public string QuantityText => Quantity.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);
    public string UnitPriceText => UnitPrice.ToString("$#,0.00", System.Globalization.CultureInfo.InvariantCulture);
    public string TotalValueText => TotalValue.ToString("$#,0.00", System.Globalization.CultureInfo.InvariantCulture);
}

public class MaterialDialog : Form
{
    private readonly string[] categoryNames;
    private readonly TextBox nameTextBox = new();
    private readonly ComboBox categoryComboBox = new();
    private readonly TextBox unitTextBox = new();
    private readonly NumericUpDown quantityInput = new();
    private readonly NumericUpDown unitPriceInput = new();
    private readonly ComboBox statusComboBox = new();

    public MaterialRecord? Material { get; private set; }

    public MaterialDialog(string title, MaterialRecord? existingMaterial = null)
        : this(title, existingMaterial, ["Cement", "Steel", "Bricks", "Aggregates"])
    {
    }

    public MaterialDialog(string title, MaterialRecord? existingMaterial, IEnumerable<string> categoryNames)
    {
        this.categoryNames = categoryNames
            .Where(categoryName => !string.IsNullOrWhiteSpace(categoryName))
            .Distinct()
            .ToArray();

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 390);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        BuildForm(existingMaterial);
    }

    private void BuildForm(MaterialRecord? existingMaterial)
    {
        Controls.Add(CreateLabel("Material Name", 28, 28));
        ConfigureTextBox(nameTextBox, 28, 58, "Enter material name");
        Controls.Add(nameTextBox);

        Controls.Add(CreateLabel("Category", 28, 102));
        ConfigureComboBox(categoryComboBox, 28, 132, this.categoryNames.Length == 0 ? ["Cement"] : this.categoryNames);
        Controls.Add(categoryComboBox);

        Controls.Add(CreateLabel("Unit", 250, 102));
        ConfigureSmallTextBox(unitTextBox, 250, 132, "Bag, Piece, m3");
        Controls.Add(unitTextBox);

        Controls.Add(CreateLabel("Quantity", 28, 176));
        ConfigureWholeNumberInput(quantityInput, 28, 206);
        Controls.Add(quantityInput);

        Controls.Add(CreateLabel("Unit Price", 250, 176));
        ConfigureMoneyInput(unitPriceInput, 250, 206);
        Controls.Add(unitPriceInput);

        Controls.Add(CreateLabel("Status", 28, 250));
        ConfigureComboBox(statusComboBox, 28, 280, ["In Stock", "Low Stock", "Out of Stock"]);
        Controls.Add(statusComboBox);

        Button saveButton = new()
        {
            Text = "Save",
            Size = new Size(100, 36),
            Left = 250,
            Top = 330,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (_, _) => SaveMaterial(existingMaterial?.Id ?? 0);
        Controls.Add(saveButton);

        Button cancelButton = new()
        {
            Text = "Cancel",
            Size = new Size(100, 36),
            Left = 360,
            Top = 330,
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Controls.Add(cancelButton);

        if (existingMaterial != null)
        {
            nameTextBox.Text = existingMaterial.Name;
            categoryComboBox.Text = existingMaterial.Category;
            unitTextBox.Text = existingMaterial.Unit;
            quantityInput.Value = existingMaterial.Quantity;
            unitPriceInput.Value = existingMaterial.UnitPrice;
            statusComboBox.Text = existingMaterial.Status;
        }
        else
        {
            categoryComboBox.SelectedIndex = 0;
            unitTextBox.Text = "Bag";
            quantityInput.Value = 1;
            unitPriceInput.Value = 1.00m;
            statusComboBox.SelectedIndex = 0;
        }
    }

    private static Label CreateLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 41, 65),
            AutoSize = false,
            Size = new Size(160, 24),
            Left = left,
            Top = top
        };
    }

    private static void ConfigureTextBox(TextBox textBox, int left, int top, string placeholder)
    {
        textBox.Left = left;
        textBox.Top = top;
        textBox.Size = new Size(412, 30);
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureSmallTextBox(TextBox textBox, int left, int top, string placeholder)
    {
        textBox.Left = left;
        textBox.Top = top;
        textBox.Size = new Size(190, 30);
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureComboBox(ComboBox comboBox, int left, int top, string[] values)
    {
        comboBox.Left = left;
        comboBox.Top = top;
        comboBox.Size = new Size(190, 30);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(values);
        comboBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureWholeNumberInput(NumericUpDown input, int left, int top)
    {
        input.Left = left;
        input.Top = top;
        input.Size = new Size(190, 30);
        input.Minimum = 0;
        input.Maximum = 1000000;
        input.ThousandsSeparator = true;
        input.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureMoneyInput(NumericUpDown input, int left, int top)
    {
        input.Left = left;
        input.Top = top;
        input.Size = new Size(190, 30);
        input.Minimum = 0;
        input.Maximum = 1000000;
        input.DecimalPlaces = 2;
        input.Increment = 0.25m;
        input.ThousandsSeparator = true;
        input.Font = new Font("Segoe UI", 10F);
    }

    private void SaveMaterial(int id)
    {
        if (string.IsNullOrWhiteSpace(nameTextBox.Text))
        {
            MessageBox.Show("Material name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(unitTextBox.Text))
        {
            MessageBox.Show("Material unit is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Material = new MaterialRecord
        {
            Id = id,
            Name = nameTextBox.Text.Trim(),
            Category = categoryComboBox.Text,
            Unit = unitTextBox.Text.Trim(),
            Quantity = (int)quantityInput.Value,
            UnitPrice = unitPriceInput.Value,
            Status = statusComboBox.Text
        };

        DialogResult = DialogResult.OK;
    }
}

public class IssueReportRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReportedDate { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    public string ReportedDateText => ReportedDate.ToString("dd/MM/yyyy");
}

public class IssueReportDialog : Form
{
    private readonly string[] projectNames;
    private readonly string[] workerNames;
    private readonly TextBox titleTextBox = new();
    private readonly ComboBox projectComboBox = new();
    private readonly ComboBox reportedByComboBox = new();
    private readonly ComboBox priorityComboBox = new();
    private readonly ComboBox statusComboBox = new();
    private readonly DateTimePicker reportedDatePicker = new();
    private readonly TextBox descriptionTextBox = new();

    public IssueReportRecord? IssueReport { get; private set; }

    public IssueReportDialog(string title)
        : this(title, ["Project Alpha", "Office Tower", "Bridge Repair", "Warehouse Extension", "Residential Complex"], ["Michael Thompson", "Sarah Johnson", "David Martinez"])
    {
    }

    public IssueReportDialog(string title, IEnumerable<string> projectNames)
        : this(title, projectNames, ["Michael Thompson", "Sarah Johnson", "David Martinez"])
    {
    }

    public IssueReportDialog(string title, IEnumerable<string> projectNames, IEnumerable<string> workerNames)
    {
        this.projectNames = projectNames
            .Where(projectName => !string.IsNullOrWhiteSpace(projectName))
            .Distinct()
            .ToArray();
        this.workerNames = workerNames
            .Where(workerName => !string.IsNullOrWhiteSpace(workerName))
            .Distinct()
            .ToArray();

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(500, 540);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        BuildForm();
    }

    private void BuildForm()
    {
        Controls.Add(CreateLabel("Issue Title", 28, 28));
        ConfigureTextBox(titleTextBox, 28, 58, "Enter issue title");
        Controls.Add(titleTextBox);

        Controls.Add(CreateLabel("Project", 28, 102));
        ConfigureComboBox(projectComboBox, 28, 132, projectNames.Length == 0 ? ["Project Alpha"] : projectNames);
        Controls.Add(projectComboBox);

        Controls.Add(CreateLabel("Reported By", 260, 102));
        ConfigureComboBox(reportedByComboBox, 260, 132, workerNames.Length == 0 ? ["Michael Thompson"] : workerNames);
        Controls.Add(reportedByComboBox);

        Controls.Add(CreateLabel("Priority", 28, 176));
        ConfigureComboBox(priorityComboBox, 28, 206, ["High", "Medium", "Low"]);
        Controls.Add(priorityComboBox);

        Controls.Add(CreateLabel("Status", 260, 176));
        ConfigureComboBox(statusComboBox, 260, 206, ["Open", "In Progress", "Resolved"]);
        Controls.Add(statusComboBox);

        Controls.Add(CreateLabel("Reported Date", 28, 250));
        reportedDatePicker.Left = 28;
        reportedDatePicker.Top = 280;
        reportedDatePicker.Size = new Size(200, 30);
        reportedDatePicker.Format = DateTimePickerFormat.Custom;
        reportedDatePicker.CustomFormat = "dd/MM/yyyy";
        reportedDatePicker.Font = new Font("Segoe UI", 10F);
        Controls.Add(reportedDatePicker);

        Controls.Add(CreateLabel("Description", 28, 324));
        descriptionTextBox.Left = 28;
        descriptionTextBox.Top = 354;
        descriptionTextBox.Size = new Size(442, 100);
        descriptionTextBox.Multiline = true;
        descriptionTextBox.PlaceholderText = "Describe the issue";
        descriptionTextBox.Font = new Font("Segoe UI", 10F);
        Controls.Add(descriptionTextBox);

        Button saveButton = new()
        {
            Text = "Save",
            Size = new Size(100, 36),
            Left = 270,
            Top = 482,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (_, _) => SaveIssueReport();
        Controls.Add(saveButton);

        Button cancelButton = new()
        {
            Text = "Cancel",
            Size = new Size(100, 36),
            Left = 380,
            Top = 482,
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Controls.Add(cancelButton);

        projectComboBox.SelectedIndex = 0;
        reportedByComboBox.SelectedIndex = 0;
        priorityComboBox.SelectedIndex = 1;
        statusComboBox.SelectedIndex = 0;
        reportedDatePicker.Value = DateTime.Today;
    }

    private static Label CreateLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 41, 65),
            AutoSize = false,
            Size = new Size(170, 24),
            Left = left,
            Top = top
        };
    }

    private static void ConfigureTextBox(TextBox textBox, int left, int top, string placeholder)
    {
        textBox.Left = left;
        textBox.Top = top;
        textBox.Size = new Size(442, 30);
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureComboBox(ComboBox comboBox, int left, int top, string[] values)
    {
        comboBox.Left = left;
        comboBox.Top = top;
        comboBox.Size = new Size(left >= 250 ? 210 : 200, 30);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(values);
        comboBox.Font = new Font("Segoe UI", 10F);
    }

    private void SaveIssueReport()
    {
        if (string.IsNullOrWhiteSpace(titleTextBox.Text))
        {
            MessageBox.Show("Issue title is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(reportedByComboBox.Text))
        {
            MessageBox.Show("Reporter name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        IssueReport = new IssueReportRecord
        {
            Title = titleTextBox.Text.Trim(),
            ProjectName = projectComboBox.Text,
            ReportedBy = reportedByComboBox.Text,
            Priority = priorityComboBox.Text,
            Status = statusComboBox.Text,
            ReportedDate = reportedDatePicker.Value.Date,
            Description = string.IsNullOrWhiteSpace(descriptionTextBox.Text) ? "No description provided." : descriptionTextBox.Text.Trim()
        };

        DialogResult = DialogResult.OK;
    }
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public string WorkerCode { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DateText => Date.ToString("dd/MM/yyyy");
}

public class AttendanceDialog : Form
{
    private readonly List<WorkerRecord> workerOptions;
    private readonly string[] projectNames;
    private readonly ComboBox workerNameComboBox = new();
    private readonly TextBox roleTextBox = new();
    private readonly ComboBox projectComboBox = new();
    private readonly DateTimePicker datePicker = new();
    private readonly TextBox checkInTextBox = new();
    private readonly TextBox checkOutTextBox = new();
    private readonly ComboBox statusComboBox = new();

    public AttendanceRecord? Attendance { get; private set; }

    public AttendanceDialog(string title, AttendanceRecord? existingRecord = null)
        : this(title, existingRecord, SeedData.CreateDefaultWorkers(), SeedData.CreateDefaultProjects().Select(project => project.Name))
    {
    }

    public AttendanceDialog(string title, AttendanceRecord? existingRecord, IEnumerable<WorkerRecord> workers, IEnumerable<string> projectNames)
    {
        workerOptions = workers.ToList();
        this.projectNames = projectNames
            .Where(projectName => !string.IsNullOrWhiteSpace(projectName))
            .Distinct()
            .ToArray();

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 505);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        BuildForm(existingRecord);
    }

    private void BuildForm(AttendanceRecord? existingRecord)
    {
        Controls.Add(CreateLabel("Worker Name", 28, 28));
        ConfigureComboBox(workerNameComboBox, 28, 58, GetWorkerNames());
        workerNameComboBox.SelectedIndexChanged += (_, _) => UpdateSelectedWorkerRole();
        Controls.Add(workerNameComboBox);

        Controls.Add(CreateLabel("Role", 28, 102));
        ConfigureTextBox(roleTextBox, 28, 132, "Enter worker role");
        roleTextBox.ReadOnly = true;
        Controls.Add(roleTextBox);

        Controls.Add(CreateLabel("Project", 28, 176));
        ConfigureComboBox(projectComboBox, 28, 206, projectNames.Length == 0 ? ["Project Alpha"] : projectNames);
        Controls.Add(projectComboBox);

        Controls.Add(CreateLabel("Date", 28, 250));
        datePicker.Left = 28;
        datePicker.Top = 280;
        datePicker.Size = new Size(190, 30);
        datePicker.Format = DateTimePickerFormat.Short;
        datePicker.Font = new Font("Segoe UI", 10F);
        Controls.Add(datePicker);

        Controls.Add(CreateLabel("Status", 250, 250));
        ConfigureComboBox(statusComboBox, 250, 280, ["Present", "Absent"]);
        Controls.Add(statusComboBox);

        Controls.Add(CreateLabel("Check In", 28, 324));
        ConfigureSmallTextBox(checkInTextBox, 28, 354, "08:00 AM");
        Controls.Add(checkInTextBox);

        Controls.Add(CreateLabel("Check Out", 250, 324));
        ConfigureSmallTextBox(checkOutTextBox, 250, 354, "05:00 PM");
        Controls.Add(checkOutTextBox);

        Button saveButton = new()
        {
            Text = "Save",
            Size = new Size(100, 36),
            Left = 250,
            Top = 446,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (_, _) => SaveAttendance(existingRecord?.Id ?? 0);
        Controls.Add(saveButton);

        Button cancelButton = new()
        {
            Text = "Cancel",
            Size = new Size(100, 36),
            Left = 360,
            Top = 446,
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Controls.Add(cancelButton);

        if (existingRecord != null)
        {
            workerNameComboBox.Text = existingRecord.WorkerName;
            roleTextBox.Text = existingRecord.Role;
            projectComboBox.Text = existingRecord.ProjectName;
            datePicker.Value = existingRecord.Date;
            checkInTextBox.Text = existingRecord.CheckIn;
            checkOutTextBox.Text = existingRecord.CheckOut;
            statusComboBox.Text = existingRecord.Status;
        }
        else
        {
            if (workerNameComboBox.Items.Count > 0)
            {
                workerNameComboBox.SelectedIndex = 0;
            }

            if (projectComboBox.Items.Count > 0)
            {
                projectComboBox.SelectedIndex = 0;
            }

            datePicker.Value = DateTime.Today;
            statusComboBox.SelectedIndex = 0;
            checkInTextBox.Text = "08:00 AM";
            checkOutTextBox.Text = "05:00 PM";
        }
    }

    private static Label CreateLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 41, 65),
            AutoSize = false,
            Size = new Size(160, 24),
            Left = left,
            Top = top
        };
    }

    private static void ConfigureTextBox(TextBox textBox, int left, int top, string placeholder)
    {
        textBox.Left = left;
        textBox.Top = top;
        textBox.Size = new Size(412, 30);
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureSmallTextBox(TextBox textBox, int left, int top, string placeholder)
    {
        textBox.Left = left;
        textBox.Top = top;
        textBox.Size = new Size(190, 30);
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureComboBox(ComboBox comboBox, int left, int top, string[] values)
    {
        comboBox.Left = left;
        comboBox.Top = top;
        comboBox.Size = new Size(left >= 250 ? 190 : 412, 30);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(values);
        comboBox.Font = new Font("Segoe UI", 10F);
    }

    private string[] GetWorkerNames()
    {
        string[] names = workerOptions
            .Select(worker => worker.Name)
            .Where(workerName => !string.IsNullOrWhiteSpace(workerName))
            .Distinct()
            .ToArray();

        return names.Length == 0 ? ["Michael Thompson"] : names;
    }

    private void UpdateSelectedWorkerRole()
    {
        WorkerRecord? selectedWorker = workerOptions.FirstOrDefault(worker => worker.Name == workerNameComboBox.Text);
        roleTextBox.Text = selectedWorker?.Role ?? roleTextBox.Text;
    }

    private void SaveAttendance(int id)
    {
        if (string.IsNullOrWhiteSpace(workerNameComboBox.Text))
        {
            MessageBox.Show("Worker name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(roleTextBox.Text))
        {
            MessageBox.Show("Worker role is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(projectComboBox.Text))
        {
            MessageBox.Show("Project name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        WorkerRecord? selectedWorker = workerOptions.FirstOrDefault(worker => worker.Name == workerNameComboBox.Text);

        Attendance = new AttendanceRecord
        {
            Id = id,
            WorkerCode = selectedWorker?.WorkerCode ?? string.Empty,
            WorkerName = workerNameComboBox.Text,
            Role = roleTextBox.Text.Trim(),
            ProjectName = projectComboBox.Text,
            Date = datePicker.Value.Date,
            CheckIn = string.IsNullOrWhiteSpace(checkInTextBox.Text) ? "-" : checkInTextBox.Text.Trim(),
            CheckOut = string.IsNullOrWhiteSpace(checkOutTextBox.Text) ? "-" : checkOutTextBox.Text.Trim(),
            Status = statusComboBox.Text
        };

        DialogResult = DialogResult.OK;
    }
}

public class TaskRecord
{
    public int Id { get; set; }
    public string TaskCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.Today;
    public string Status { get; set; } = string.Empty;
    public string DueDateText => DueDate.ToString("dd/MM/yyyy");
}

public class TaskDialog : Form
{
    private readonly string[] projectNames;
    private readonly string[] workerNames;
    private readonly TextBox titleTextBox = new();
    private readonly ComboBox projectComboBox = new();
    private readonly ComboBox assignedToComboBox = new();
    private readonly ComboBox priorityComboBox = new();
    private readonly DateTimePicker dueDatePicker = new();
    private readonly ComboBox statusComboBox = new();

    public TaskRecord? Task { get; private set; }

    public TaskDialog(string title, TaskRecord? existingTask = null)
        : this(title, existingTask, SeedData.CreateDefaultProjects().Select(project => project.Name), SeedData.CreateDefaultWorkers().Select(worker => worker.Name))
    {
    }

    public TaskDialog(string title, TaskRecord? existingTask, IEnumerable<string> projectNames, IEnumerable<string> workerNames)
    {
        this.projectNames = projectNames
            .Where(projectName => !string.IsNullOrWhiteSpace(projectName))
            .Distinct()
            .ToArray();
        this.workerNames = workerNames
            .Where(workerName => !string.IsNullOrWhiteSpace(workerName))
            .Distinct()
            .ToArray();

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(460, 430);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        BuildForm(existingTask);
    }

    private void BuildForm(TaskRecord? existingTask)
    {
        Controls.Add(CreateLabel("Task Name", 28, 28));
        ConfigureTextBox(titleTextBox, 28, 58, "Enter task name");
        Controls.Add(titleTextBox);

        Controls.Add(CreateLabel("Project", 28, 102));
        ConfigureComboBox(projectComboBox, 28, 132, this.projectNames.Length == 0 ? ["Project Alpha"] : this.projectNames);
        projectComboBox.Width = 402;
        Controls.Add(projectComboBox);

        Controls.Add(CreateLabel("Assigned To", 28, 176));
        ConfigureComboBox(assignedToComboBox, 28, 206, this.workerNames.Length == 0 ? ["Michael Thompson"] : this.workerNames);
        assignedToComboBox.Width = 402;
        Controls.Add(assignedToComboBox);

        Controls.Add(CreateLabel("Priority", 28, 250));
        ConfigureComboBox(priorityComboBox, 28, 280, ["High", "Medium", "Low"]);
        Controls.Add(priorityComboBox);

        Controls.Add(CreateLabel("Due Date", 250, 250));
        dueDatePicker.Left = 250;
        dueDatePicker.Top = 280;
        dueDatePicker.Size = new Size(180, 30);
        dueDatePicker.Format = DateTimePickerFormat.Short;
        dueDatePicker.Font = new Font("Segoe UI", 10F);
        Controls.Add(dueDatePicker);

        Controls.Add(CreateLabel("Status", 28, 324));
        ConfigureComboBox(statusComboBox, 28, 354, ["Pending", "In Progress", "Completed", "Overdue"]);
        Controls.Add(statusComboBox);

        Button saveButton = new()
        {
            Text = "Save",
            Size = new Size(100, 36),
            Left = 230,
            Top = 386,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (_, _) => SaveTask(existingTask?.Id ?? 0);
        Controls.Add(saveButton);

        Button cancelButton = new()
        {
            Text = "Cancel",
            Size = new Size(100, 36),
            Left = 340,
            Top = 386,
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Controls.Add(cancelButton);

        if (existingTask != null)
        {
            titleTextBox.Text = existingTask.Title;
            projectComboBox.Text = existingTask.ProjectName;
            assignedToComboBox.Text = existingTask.AssignedTo;
            priorityComboBox.Text = existingTask.Priority;
            dueDatePicker.Value = existingTask.DueDate;
            statusComboBox.Text = existingTask.Status;
        }
        else
        {
            projectComboBox.SelectedIndex = 0;
            assignedToComboBox.SelectedIndex = 0;
            priorityComboBox.SelectedIndex = 1;
            dueDatePicker.Value = DateTime.Today.AddDays(7);
            statusComboBox.SelectedIndex = 0;
        }
    }

    private static Label CreateLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 41, 65),
            AutoSize = false,
            Size = new Size(160, 24),
            Left = left,
            Top = top
        };
    }

    private static void ConfigureTextBox(TextBox textBox, int left, int top, string placeholder)
    {
        textBox.Left = left;
        textBox.Top = top;
        textBox.Size = new Size(402, 30);
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureComboBox(ComboBox comboBox, int left, int top, string[] values)
    {
        comboBox.Left = left;
        comboBox.Top = top;
        comboBox.Size = new Size(left >= 230 ? 180 : 190, 30);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(values);
        comboBox.Font = new Font("Segoe UI", 10F);
    }

    private void SaveTask(int id)
    {
        if (string.IsNullOrWhiteSpace(titleTextBox.Text))
        {
            MessageBox.Show("Task name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(projectComboBox.Text))
        {
            MessageBox.Show("Project name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(assignedToComboBox.Text))
        {
            MessageBox.Show("Assigned worker name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Task = new TaskRecord
        {
            Id = id,
            Title = titleTextBox.Text.Trim(),
            ProjectName = projectComboBox.Text,
            AssignedTo = assignedToComboBox.Text,
            Priority = priorityComboBox.Text,
            DueDate = dueDatePicker.Value.Date,
            Status = statusComboBox.Text
        };

        DialogResult = DialogResult.OK;
    }
}

public class ProjectRecord
{
    public int Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today;
    public string Manager { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string StartDateText => StartDate.ToString("dd/MM/yyyy");
    public string EndDateText => EndDate.ToString("dd/MM/yyyy");
}

public class ProjectDialog : Form
{
    private readonly string[] managerNames;
    private readonly TextBox nameTextBox = new();
    private readonly TextBox locationTextBox = new();
    private readonly ComboBox managerComboBox = new();
    private readonly DateTimePicker startDatePicker = new();
    private readonly DateTimePicker endDatePicker = new();
    private readonly ComboBox statusComboBox = new();
    private readonly NumericUpDown progressInput = new();

    public ProjectRecord? Project { get; private set; }

    public ProjectDialog(string title, ProjectRecord? existingProject = null)
        : this(title, existingProject, SeedData.CreateDefaultWorkers().Select(worker => worker.Name))
    {
    }

    public ProjectDialog(string title, ProjectRecord? existingProject, IEnumerable<string> managerNames)
    {
        this.managerNames = managerNames
            .Where(managerName => !string.IsNullOrWhiteSpace(managerName))
            .Distinct()
            .ToArray();

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 455);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        BuildForm(existingProject);
    }

    private void BuildForm(ProjectRecord? existingProject)
    {
        Controls.Add(CreateLabel("Project Name", 28, 28));
        ConfigureTextBox(nameTextBox, 28, 58, "Enter project name");
        Controls.Add(nameTextBox);

        Controls.Add(CreateLabel("Location", 28, 102));
        ConfigureTextBox(locationTextBox, 28, 132, "Enter location");
        Controls.Add(locationTextBox);

        Controls.Add(CreateLabel("Manager", 28, 176));
        ConfigureComboBox(managerComboBox, 28, 206, this.managerNames.Length == 0 ? ["John Anderson"] : this.managerNames);
        managerComboBox.Width = 412;
        Controls.Add(managerComboBox);

        Controls.Add(CreateLabel("Start Date", 28, 250));
        ConfigureDatePicker(startDatePicker, 28, 280);
        Controls.Add(startDatePicker);

        Controls.Add(CreateLabel("End Date", 250, 250));
        ConfigureDatePicker(endDatePicker, 250, 280);
        Controls.Add(endDatePicker);

        Controls.Add(CreateLabel("Status", 28, 324));
        ConfigureComboBox(statusComboBox, 28, 354, ["Planning", "In Progress", "Completed", "Delayed"]);
        Controls.Add(statusComboBox);

        Controls.Add(CreateLabel("Progress", 250, 324));
        progressInput.Left = 250;
        progressInput.Top = 354;
        progressInput.Size = new Size(190, 30);
        progressInput.Minimum = 0;
        progressInput.Maximum = 100;
        progressInput.Font = new Font("Segoe UI", 10F);
        Controls.Add(progressInput);

        Button saveButton = new()
        {
            Text = "Save",
            Size = new Size(100, 36),
            Left = 250,
            Top = 406,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (_, _) => SaveProject(existingProject?.Id ?? 0);
        Controls.Add(saveButton);

        Button cancelButton = new()
        {
            Text = "Cancel",
            Size = new Size(100, 36),
            Left = 360,
            Top = 406,
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Controls.Add(cancelButton);

        if (existingProject != null)
        {
            nameTextBox.Text = existingProject.Name;
            locationTextBox.Text = existingProject.Location;
            managerComboBox.Text = existingProject.Manager;
            startDatePicker.Value = existingProject.StartDate;
            endDatePicker.Value = existingProject.EndDate;
            statusComboBox.Text = existingProject.Status;
            progressInput.Value = existingProject.Progress;
        }
        else
        {
            managerComboBox.SelectedIndex = 0;
            startDatePicker.Value = DateTime.Today;
            endDatePicker.Value = DateTime.Today.AddMonths(3);
            statusComboBox.SelectedIndex = 0;
            progressInput.Value = 0;
        }
    }

    private static Label CreateLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 41, 65),
            AutoSize = false,
            Size = new Size(180, 24),
            Left = left,
            Top = top
        };
    }

    private static void ConfigureTextBox(TextBox textBox, int left, int top, string placeholder)
    {
        textBox.Left = left;
        textBox.Top = top;
        textBox.Size = new Size(412, 30);
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureDatePicker(DateTimePicker datePicker, int left, int top)
    {
        datePicker.Left = left;
        datePicker.Top = top;
        datePicker.Size = new Size(190, 30);
        datePicker.Format = DateTimePickerFormat.Short;
        datePicker.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureComboBox(ComboBox comboBox, int left, int top, string[] values)
    {
        comboBox.Left = left;
        comboBox.Top = top;
        comboBox.Size = new Size(190, 30);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(values);
        comboBox.Font = new Font("Segoe UI", 10F);
    }

    private void SaveProject(int id)
    {
        if (string.IsNullOrWhiteSpace(nameTextBox.Text))
        {
            MessageBox.Show("Project name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(locationTextBox.Text))
        {
            MessageBox.Show("Project location is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (endDatePicker.Value.Date < startDatePicker.Value.Date)
        {
            MessageBox.Show("End date cannot be before start date.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Project = new ProjectRecord
        {
            Id = id,
            Name = nameTextBox.Text.Trim(),
            Location = locationTextBox.Text.Trim(),
            Manager = managerComboBox.Text,
            StartDate = startDatePicker.Value.Date,
            EndDate = endDatePicker.Value.Date,
            Status = statusComboBox.Text,
            Progress = (int)progressInput.Value
        };

        DialogResult = DialogResult.OK;
    }
}

public class WorkerRecord
{
    public int Id { get; set; }
    public string WorkerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Today;
    public string Status { get; set; } = string.Empty;
    public string HireDateText => HireDate.ToString("dd/MM/yyyy");
}

public class WorkerDialog : Form
{
    private readonly TextBox nameTextBox = new();
    private readonly TextBox phoneTextBox = new();
    private readonly TextBox emailTextBox = new();
    private readonly DateTimePicker hireDatePicker = new();
    private readonly ComboBox roleComboBox = new();
    private readonly ComboBox statusComboBox = new();

    public WorkerRecord? Worker { get; private set; }

    public WorkerDialog(string title, WorkerRecord? existingWorker = null)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(460, 430);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        BuildForm(existingWorker);
    }

    private void BuildForm(WorkerRecord? existingWorker)
    {
        Controls.Add(CreateLabel("Name", 28, 28));
        ConfigureTextBox(nameTextBox, 28, 58, "Enter worker name");
        Controls.Add(nameTextBox);

        Controls.Add(CreateLabel("Role", 28, 102));
        ConfigureComboBox(roleComboBox, 28, 132, ["Mason", "Electrician", "Carpenter", "Plumber", "Safety Officer", "Supervisor"]);
        Controls.Add(roleComboBox);

        Controls.Add(CreateLabel("Phone", 28, 176));
        ConfigureTextBox(phoneTextBox, 28, 206, "Enter phone number");
        Controls.Add(phoneTextBox);

        Controls.Add(CreateLabel("Email", 28, 250));
        ConfigureTextBox(emailTextBox, 28, 280, "Enter email address");
        Controls.Add(emailTextBox);

        Controls.Add(CreateLabel("Hire Date", 28, 324));
        hireDatePicker.Left = 28;
        hireDatePicker.Top = 354;
        hireDatePicker.Size = new Size(190, 30);
        hireDatePicker.Format = DateTimePickerFormat.Short;
        Controls.Add(hireDatePicker);

        Controls.Add(CreateLabel("Status", 250, 324));
        ConfigureComboBox(statusComboBox, 250, 354, ["Active", "On Leave", "Inactive"]);
        Controls.Add(statusComboBox);

        Button saveButton = new()
        {
            Text = "Save",
            Size = new Size(100, 36),
            Left = 230,
            Top = 386,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (_, _) => SaveWorker(existingWorker?.Id ?? 0);
        Controls.Add(saveButton);

        Button cancelButton = new()
        {
            Text = "Cancel",
            Size = new Size(100, 36),
            Left = 340,
            Top = 386,
            BackColor = Color.FromArgb(107, 114, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Controls.Add(cancelButton);

        if (existingWorker != null)
        {
            nameTextBox.Text = existingWorker.Name;
            roleComboBox.Text = existingWorker.Role;
            phoneTextBox.Text = existingWorker.Phone;
            emailTextBox.Text = existingWorker.Email;
            hireDatePicker.Value = existingWorker.HireDate;
            statusComboBox.Text = existingWorker.Status;
        }
        else
        {
            roleComboBox.SelectedIndex = 0;
            statusComboBox.SelectedIndex = 0;
        }
    }

    private static Label CreateLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 41, 65),
            AutoSize = false,
            Size = new Size(160, 24),
            Left = left,
            Top = top
        };
    }

    private static void ConfigureTextBox(TextBox textBox, int left, int top, string placeholder)
    {
        textBox.Left = left;
        textBox.Top = top;
        textBox.Size = new Size(360, 30);
        textBox.PlaceholderText = placeholder;
        textBox.Font = new Font("Segoe UI", 10F);
    }

    private static void ConfigureComboBox(ComboBox comboBox, int left, int top, string[] values)
    {
        comboBox.Left = left;
        comboBox.Top = top;
        comboBox.Size = new Size(left >= 230 ? 180 : 360, 30);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(values);
        comboBox.Font = new Font("Segoe UI", 10F);
    }

    private void SaveWorker(int id)
    {
        if (string.IsNullOrWhiteSpace(nameTextBox.Text))
        {
            MessageBox.Show("Worker name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(phoneTextBox.Text))
        {
            MessageBox.Show("Phone number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(emailTextBox.Text))
        {
            MessageBox.Show("Email address is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Worker = new WorkerRecord
        {
            Id = id,
            Name = nameTextBox.Text.Trim(),
            Role = roleComboBox.Text,
            Phone = phoneTextBox.Text.Trim(),
            Email = emailTextBox.Text.Trim(),
            HireDate = hireDatePicker.Value.Date,
            Status = statusComboBox.Text
        };

        DialogResult = DialogResult.OK;
    }
}

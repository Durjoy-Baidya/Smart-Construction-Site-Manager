using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartConstructionSiteManagement;

public partial class DashboardForm
{
    private void ShowMaterialsContent()
    {
        contentPanel.Controls.Clear();

        contentPanel.Controls.Add(CreateModuleTitle("Materials Management"));

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
            PlaceholderText = "Search materials...",
            Font = new Font("Segoe UI", 11F),
            Size = new Size(480, 34),
            Left = 16,
            Top = 18
        };
        filterPanel.Controls.Add(searchTextBox);

        ComboBox categoryComboBox = CreateFilterComboBox("Category: All", 520);
        categoryComboBox.Items.Add("Category: All");
        categoryComboBox.Items.AddRange(materials.Select(material => material.Category).Distinct().ToArray());
        categoryComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(categoryComboBox);

        ComboBox statusComboBox = CreateFilterComboBox("Status: All", 770);
        statusComboBox.Items.AddRange(["Status: All", "In Stock", "Low Stock", "Out of Stock"]);
        statusComboBox.SelectedIndex = 0;
        filterPanel.Controls.Add(statusComboBox);

        Button addButton = CreateActionButton("Add Material", PrimaryBlue, 0);
        addButton.Size = new Size(140, 36);
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

        RoundedPanel materialsTable = CreateWorkersTablePanel();
        contentPanel.Controls.Add(materialsTable);

        Label showingLabel = new()
        {
            Text = "Showing materials",
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(280, 28),
            Left = 34,
            Top = 730
        };
        contentPanel.Controls.Add(showingLabel);

        void RefreshMaterialStats()
        {
            decimal totalValue = materials.Sum(material => material.Quantity * material.UnitPrice);

            statPanel.Controls.Clear();
            statPanel.Controls.Add(CreateMaterialStatCard("Total Materials", materials.Count.ToString(), "types", Color.FromArgb(52, 152, 219)));
            statPanel.Controls.Add(CreateMaterialStatCard("Total Stock", materials.Sum(material => material.Quantity).ToString("N0"), "units", Green));
            statPanel.Controls.Add(CreateMaterialStatCard("Low Stock Items", materials.Count(material => material.Status == "Low Stock").ToString(), "items", Orange));
            statPanel.Controls.Add(CreateMaterialStatCard("Total Value", $"${totalValue:N0}", "USD", Color.FromArgb(168, 85, 247)));
        }

        void RefreshMaterialsTable()
        {
            List<MaterialRecord> filteredMaterials = GetFilteredMaterials(searchTextBox.Text, categoryComboBox.Text, statusComboBox.Text);
            materialsTable.Height = GetManagementTableHeight(filteredMaterials.Count);
            PopulateMaterialsTable(materialsTable, filteredMaterials, RefreshMaterialsTable);
            showingLabel.Top = materialsTable.Bottom + 20;
            showingLabel.Text = $"Showing {filteredMaterials.Count} of {materials.Count} materials";
            RefreshMaterialStats();
        }

        void ArrangeMaterialsPage()
        {
            filterPanel.Width = contentPanel.ClientSize.Width - 68;
            addButton.Left = filterPanel.Width - addButton.Width - 16;
            statusComboBox.Left = addButton.Left - statusComboBox.Width - 20;
            categoryComboBox.Left = statusComboBox.Left - categoryComboBox.Width - 16;
            searchTextBox.Width = Math.Min(520, Math.Max(320, categoryComboBox.Left - searchTextBox.Left - 28));
            statPanel.Width = contentPanel.ClientSize.Width - 68;
            List<MaterialRecord> filteredMaterials = GetFilteredMaterials(searchTextBox.Text, categoryComboBox.Text, statusComboBox.Text);
            materialsTable.SetBounds(34, 315, contentPanel.ClientSize.Width - 68, GetManagementTableHeight(filteredMaterials.Count));
            PopulateMaterialsTable(materialsTable, filteredMaterials, RefreshMaterialsTable);
            showingLabel.Top = materialsTable.Bottom + 20;
        }

        searchTextBox.TextChanged += (_, _) => RefreshMaterialsTable();
        categoryComboBox.SelectedIndexChanged += (_, _) => RefreshMaterialsTable();
        statusComboBox.SelectedIndexChanged += (_, _) => RefreshMaterialsTable();
        addButton.Click += (_, _) => AddMaterial(RefreshMaterialsTable);
        contentPanel.Resize += (_, _) => ArrangeMaterialsPage();
        RefreshMaterialStats();
        ArrangeMaterialsPage();
        RefreshMaterialsTable();
    }

    private Panel CreateMaterialStatCard(string title, string value, string suffix, Color valueColor)
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

        Label valueLabel = new()
        {
            Text = value,
            Font = new Font("Segoe UI", 20F),
            ForeColor = valueColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Size = new Size(140, 42),
            Left = 40,
            Top = 45
        };
        card.Controls.Add(valueLabel);

        card.Controls.Add(new Label
        {
            Text = suffix,
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Size = new Size(55, 28),
            Left = 185,
            Top = 55
        });

        return card;
    }

    private void PopulateMaterialsTable(RoundedPanel tablePanel, List<MaterialRecord> filteredMaterials, Action refreshTable)
    {
        tablePanel.SuspendLayout();
        tablePanel.Controls.Clear();

        int[] columnWidths = GetMaterialColumnWidths(tablePanel.Width);
        string[] headers = ["Material Name", "Category", "Unit", "Quantity", "Unit Price", "Total Value", "Status", "Actions"];

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
        foreach (MaterialRecord material in filteredMaterials)
        {
            tablePanel.Controls.Add(CreateMaterialTableRow(material, rowTop, columnWidths, tablePanel.Width - 2, refreshTable));
            rowTop += 68;
        }

        tablePanel.ResumeLayout();
    }

    private static int[] GetMaterialColumnWidths(int tableWidth)
    {
        int contentWidth = Math.Max(1115, tableWidth - 28);
        int[] baseWidths = [210, 145, 95, 105, 115, 140, 125, 180];
        double scale = contentWidth / 1115.0;

        return baseWidths
            .Select(width => (int)Math.Round(width * scale))
            .ToArray();
    }

    private Panel CreateMaterialTableRow(MaterialRecord material, int top, int[] columnWidths, int rowWidth, Action refreshTable)
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
        rowPanel.Controls.Add(CreateTableCell(material.Name, left, columnWidths[0]));
        left += columnWidths[0];
        rowPanel.Controls.Add(CreateTableCell(material.Category, left, columnWidths[1]));
        left += columnWidths[1];
        rowPanel.Controls.Add(CreateTableCell(material.Unit, left, columnWidths[2]));
        left += columnWidths[2];
        rowPanel.Controls.Add(CreateTableCell(material.QuantityText, left, columnWidths[3]));
        left += columnWidths[3];
        rowPanel.Controls.Add(CreateTableCell(material.UnitPriceText, left, columnWidths[4]));
        left += columnWidths[4];
        rowPanel.Controls.Add(CreateTableCell(material.TotalValueText, left, columnWidths[5]));
        left += columnWidths[5];
        rowPanel.Controls.Add(CreateMaterialStatusBadge(material.Status, left + 4, 19));
        left += columnWidths[6];

        Button editButton = CreateRowActionButton("Edit", PrimaryBlue, left + 6);
        editButton.Click += (_, _) => EditMaterial(material, refreshTable);
        rowPanel.Controls.Add(editButton);

        Button deleteButton = CreateRowActionButton("Delete", Red, left + 76);
        deleteButton.Click += (_, _) => DeleteMaterial(material, refreshTable);
        rowPanel.Controls.Add(deleteButton);

        AddTableRowSeparator(rowPanel);
        return rowPanel;
    }

    private Label CreateMaterialStatusBadge(string status, int left, int top)
    {
        Color backColor = status switch
        {
            "In Stock" => Color.FromArgb(220, 252, 231),
            "Low Stock" => Color.FromArgb(255, 237, 213),
            _ => Color.FromArgb(254, 226, 226)
        };

        Color foreColor = status switch
        {
            "In Stock" => Color.FromArgb(22, 101, 52),
            "Low Stock" => Color.FromArgb(180, 83, 9),
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
            Size = new Size(status == "Low Stock" ? 92 : 82, 30),
            Left = left,
            Top = top
        };
    }

    private List<MaterialRecord> GetFilteredMaterials(string searchText, string categoryFilter, string statusFilter)
    {
        string normalizedSearch = searchText.Trim().ToLowerInvariant();
        string selectedCategory = categoryFilter.Replace("Category: ", "");
        string selectedStatus = statusFilter.Replace("Status: ", "");

        return materials
            .Where(material =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                material.Name.ToLowerInvariant().Contains(normalizedSearch) ||
                material.Category.ToLowerInvariant().Contains(normalizedSearch) ||
                material.Unit.ToLowerInvariant().Contains(normalizedSearch) ||
                material.Status.ToLowerInvariant().Contains(normalizedSearch))
            .Where(material => selectedCategory == "All" || material.Category == selectedCategory)
            .Where(material => selectedStatus == "All" || material.Status == selectedStatus)
            .ToList();
    }

    private void AddMaterial(Action refreshTable)
    {
        using MaterialDialog dialog = new("Add Material", null, materials.Select(material => material.Category));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Material == null)
        {
            return;
        }

        int nextId = materials.Count == 0 ? 1 : materials.Max(material => material.Id) + 1;
        dialog.Material.Id = nextId;
        materials.Add(dialog.Material);
        SaveApplicationData();
        refreshTable();
    }

    private void EditMaterial(MaterialRecord material, Action refreshTable)
    {
        using MaterialDialog dialog = new("Edit Material", material, materials.Select(item => item.Category));

        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Material == null)
        {
            return;
        }

        material.Name = dialog.Material.Name;
        material.Category = dialog.Material.Category;
        material.Unit = dialog.Material.Unit;
        material.Quantity = dialog.Material.Quantity;
        material.UnitPrice = dialog.Material.UnitPrice;
        material.Status = dialog.Material.Status;
        SaveApplicationData();
        refreshTable();
    }

    private void DeleteMaterial(MaterialRecord material, Action refreshTable)
    {
        DialogResult result = MessageBox.Show(
            $"Delete material '{material.Name}'?",
            "Delete Material",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        materials.Remove(material);
        SaveApplicationData();
        refreshTable();
    }
}

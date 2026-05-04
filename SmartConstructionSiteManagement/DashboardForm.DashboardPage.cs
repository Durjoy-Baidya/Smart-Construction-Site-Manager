using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartConstructionSiteManagement;

public partial class DashboardForm
{
    private void ShowDashboardContent()
    {
        contentPanel.Controls.Clear();

        FlowLayoutPanel metricsPanel = new()
        {
            Location = new Point(34, 28),
            Size = new Size(contentPanel.Width - 68, 150),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Height = 172,
            BackColor = PageBackColor,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        contentPanel.Controls.Add(metricsPanel);

        int activeWorkers = workers.Count(worker => worker.Status == "Active");
        int onLeaveWorkers = workers.Count(worker => worker.Status == "On Leave");
        int activeTasks = tasks.Count(task => task.Status is "Pending" or "In Progress");
        int overdueTasks = tasks.Count(task => task.Status == "Overdue");
        int inProgressProjects = projects.Count(project => project.Status == "In Progress");
        int stockedMaterials = materials.Count(material => material.Status == "In Stock");
        int materialStockRate = materials.Count == 0 ? 0 : (int)Math.Round(stockedMaterials * 100m / materials.Count);

        metricsPanel.Controls.Add(CreateMetricCard("Total Workers", workers.Count.ToString(), $"{onLeaveWorkers} on leave", Color.FromArgb(52, 152, 219)));
        metricsPanel.Controls.Add(CreateMetricCard("Active Tasks", activeTasks.ToString(), $"{overdueTasks} overdue", Red));
        metricsPanel.Controls.Add(CreateMetricCard("Total Projects", projects.Count.ToString(), $"{inProgressProjects} in progress", Green));
        metricsPanel.Controls.Add(CreateMetricCard("Materials Stock", $"{materialStockRate}%", GetMaterialStockSubtitle(materialStockRate), Orange));

        RoundedPanel recentActivityPanel = CreateRecentActivityPanel();
        RoundedPanel quickStatsPanel = CreateQuickStatsPanel();
        contentPanel.Controls.Add(recentActivityPanel);
        contentPanel.Controls.Add(quickStatsPanel);

        void ArrangeLowerPanels()
        {
            int contentWidth = contentPanel.ClientSize.Width - 68;
            int gap = 24;
            int recentWidth = (contentWidth - gap) * 48 / 100;
            int quickWidth = contentWidth - gap - recentWidth;

            recentActivityPanel.SetBounds(34, 206, recentWidth, 370);
            quickStatsPanel.SetBounds(34 + recentWidth + gap, 206, quickWidth, 370);
            ArrangeQuickStats(quickStatsPanel);
        }

        contentPanel.Resize += (_, _) => ArrangeLowerPanels();
        ArrangeLowerPanels();
    }

    private Panel CreateMetricCard(string title, string value, string subtitle, Color valueColor)
    {
        RoundedPanel card = new()
        {
            Size = new Size(235, 145),
            Margin = new Padding(0, 0, 24, 0),
            BackColor = Color.White,
            BorderRadius = 4,
            BorderColor = BorderColor
        };

        card.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 11F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(180, 28),
            Left = 24,
            Top = 28
        });

        card.Controls.Add(new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 28F),
            ForeColor = valueColor,
            AutoSize = false,
            Size = new Size(180, 54),
            Left = 24,
            Top = 58
        });

        card.Controls.Add(new Label
        {
            Text = subtitle,
            Font = new Font("Segoe UI", 10F),
            ForeColor = MutedTextColor,
            AutoSize = false,
            Size = new Size(180, 24),
            Left = 24,
            Top = 112
        });

        return card;
    }

    private RoundedPanel CreateRecentActivityPanel()
    {
        RoundedPanel panel = new()
        {
            BackColor = Color.White,
            BorderRadius = 4,
            BorderColor = BorderColor
        };

        panel.Controls.Add(CreatePanelTitle("Recent Activity"));
        panel.Controls.Add(CreateActivityItem(GetLatestTaskActivity(), "Latest task update", PrimaryBlue, 80));
        panel.Controls.Add(CreateActivityItem(GetLatestMaterialActivity(), "Material inventory", Green, 170));
        panel.Controls.Add(CreateActivityItem(GetLatestIssueActivity(), "Issue reporting", Orange, 260));
        panel.Resize += (_, _) => ArrangeRecentActivity(panel);
        ArrangeRecentActivity(panel);

        return panel;
    }

    private void ArrangeRecentActivity(Control panel)
    {
        foreach (Control child in panel.Controls)
        {
            if (child.Tag as string != "activity-row")
            {
                continue;
            }

            child.Width = Math.Max(260, panel.Width - 64);

            foreach (Label label in child.Controls.OfType<Label>())
            {
                label.Width = Math.Max(180, child.Width - label.Left - 8);
            }
        }
    }

    private RoundedPanel CreateQuickStatsPanel()
    {
        RoundedPanel panel = new()
        {
            BackColor = Color.White,
            BorderRadius = 4,
            BorderColor = BorderColor
        };

        panel.Controls.Add(CreatePanelTitle("Quick Stats"));
        int projectCompletion = projects.Count == 0 ? 0 : (int)Math.Round(projects.Average(project => project.Progress));
        int presentRecords = attendanceRecords.Count(record => record.Status == "Present");
        int attendanceRate = attendanceRecords.Count == 0 ? 0 : (int)Math.Round(presentRecords * 100m / attendanceRecords.Count);
        int completedTasks = tasks.Count(task => task.Status == "Completed");
        int taskCompletion = tasks.Count == 0 ? 0 : (int)Math.Round(completedTasks * 100m / tasks.Count);

        panel.Controls.Add(CreateStatBar("Project Completion", $"{projectCompletion}%", Green, 86));
        panel.Controls.Add(CreateStatBar("Worker Attendance", $"{attendanceRate}%", PrimaryBlue, 180));
        panel.Controls.Add(CreateStatBar("Task Completion", $"{taskCompletion}%", Orange, 274));
        panel.Resize += (_, _) => ArrangeQuickStats(panel);
        ArrangeQuickStats(panel);

        return panel;
    }

    private static string GetMaterialStockSubtitle(int stockRate)
    {
        return stockRate >= 75 ? "Stock healthy" : stockRate >= 40 ? "Needs review" : "Low stock";
    }

    private string GetLatestTaskActivity()
    {
        TaskRecord? task = tasks.OrderByDescending(task => task.DueDate).FirstOrDefault();
        return task == null ? "No task records available" : $"{task.Title} - {task.Status}";
    }

    private string GetLatestMaterialActivity()
    {
        MaterialRecord? material = materials.OrderBy(material => material.Quantity).FirstOrDefault();
        return material == null ? "No material records available" : $"{material.Name} stock: {material.QuantityText}";
    }

    private string GetLatestIssueActivity()
    {
        IssueReportRecord? issue = issueReports.OrderByDescending(issue => issue.ReportedDate).FirstOrDefault();
        return issue == null ? "No issue reports available" : $"{issue.Title} - {issue.Status}";
    }

    private void ArrangeQuickStats(Control panel)
    {
        foreach (Control child in panel.Controls)
        {
            if (child.Tag as string != "stat-row")
            {
                continue;
            }

            child.Width = Math.Max(280, panel.Width - 48);

            Label percentLabel = child.Controls.OfType<Label>().First(label => label.Tag as string == "percent");
            ProgressLine progressLine = child.Controls.OfType<ProgressLine>().First();

            percentLabel.Left = child.Width - percentLabel.Width;
            progressLine.Width = child.Width;
            progressLine.Invalidate();
        }
    }

    private Label CreatePanelTitle(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = false,
            Size = new Size(280, 34),
            Left = 24,
            Top = 30
        };
    }
}

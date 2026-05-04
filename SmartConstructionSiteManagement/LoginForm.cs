using System.Drawing.Drawing2D;

namespace SmartConstructionSiteManagement;

public partial class LoginForm : Form
{
    private readonly IAppDataStore appDataStore;
    private readonly AuthenticationService authenticationService;
    private TextBox emailTextBox = null!;
    private TextBox passwordTextBox = null!;
    private EyeToggleButton eyeButton = null!;
    private bool passwordHidden = true;

    private static readonly Color PageBackColor = Color.FromArgb(248, 250, 252);
    private static readonly Color TextColor = Color.FromArgb(25, 41, 65);
    private static readonly Color MutedTextColor = Color.FromArgb(105, 116, 133);
    private static readonly Color BorderColor = Color.FromArgb(214, 221, 230);
    private static readonly Color PrimaryBlue = Color.FromArgb(37, 99, 235);

    public LoginForm()
        : this(new SqliteAppDataStore())
    {
    }

    public LoginForm(IAppDataStore appDataStore)
    {
        this.appDataStore = appDataStore;
        authenticationService = new AuthenticationService(appDataStore);
        InitializeComponent();
        BuildLoginUI();
    }

    private void BuildLoginUI()
    {
        SuspendLayout();
        Controls.Clear();

        Text = "Smart Construction Site Management - Login";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1180, 820);
        MinimumSize = new Size(900, 650);
        BackColor = PageBackColor;
        Font = new Font("Segoe UI", 10F);

        RoundedPanel loginCard = new()
        {
            Size = new Size(780, 720),
            BackColor = Color.White,
            BorderRadius = 7,
            BorderColor = Color.FromArgb(222, 226, 232),
            ShadowColor = Color.FromArgb(35, 148, 163, 184),
            ShadowSize = 18,
            Anchor = AnchorStyles.None
        };
        CenterCard(loginCard);
        Controls.Add(loginCard);

        PictureBox logoBox = new()
        {
            Size = new Size(90, 90),
            Left = (loginCard.Width - 90) / 2,
            Top = 58,
            SizeMode = PictureBoxSizeMode.StretchImage,
            Image = LoadLogoImage()
        };
        loginCard.Controls.Add(logoBox);

        Label titleLabel = CreateCenteredLabel(
            "Smart Construction Site Management",
            new Font("Segoe UI", 20F, FontStyle.Bold),
            TextColor,
            168,
            54);
        loginCard.Controls.Add(titleLabel);

        Label subtitleLabel = CreateCenteredLabel(
            "Please sign in to continue",
            new Font("Segoe UI", 14F),
            MutedTextColor,
            218,
            38);
        loginCard.Controls.Add(subtitleLabel);

        Label emailLabel = CreateFieldLabel("Email", 80, 300);
        loginCard.Controls.Add(emailLabel);

        emailTextBox = CreateInputTextBox("Enter your email", 80, 342);
        loginCard.Controls.Add(emailTextBox.Parent!);

        Label passwordLabel = CreateFieldLabel("Password", 80, 420);
        loginCard.Controls.Add(passwordLabel);

        RoundedInputPanel passwordPanel = CreateInputPanel(80, 462);
        loginCard.Controls.Add(passwordPanel);

        passwordTextBox = CreateInnerTextBox("Enter your password");
        passwordTextBox.UseSystemPasswordChar = true;
        passwordTextBox.Width = 510;
        passwordPanel.Controls.Add(passwordTextBox);

        eyeButton = new EyeToggleButton
        {
            Size = new Size(38, 34),
            Left = passwordPanel.Width - 54,
            Top = 11,
            BackColor = Color.White,
            IconColor = Color.FromArgb(64, 77, 97),
            IsPasswordHidden = passwordHidden,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        eyeButton.Click += TogglePasswordVisibility;
        passwordPanel.Controls.Add(eyeButton);

        RoundedButton loginButton = new()
        {
            Text = "Login",
            Font = new Font("Segoe UI", 14F),
            Size = new Size(620, 56),
            Left = 80,
            Top = 550,
            BackColor = PrimaryBlue,
            ForeColor = Color.White,
            BorderRadius = 5,
            Cursor = Cursors.Hand
        };
        loginButton.Click += OpenDashboard;
        loginCard.Controls.Add(loginButton);

        LinkLabel forgotPasswordLink = new()
        {
            Text = "Forgot Password?",
            Font = new Font("Segoe UI", 13F),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(620, 34),
            Left = 80,
            Top = 632,
            LinkColor = PrimaryBlue,
            ActiveLinkColor = Color.FromArgb(29, 78, 216),
            VisitedLinkColor = PrimaryBlue
        };
        forgotPasswordLink.LinkBehavior = LinkBehavior.NeverUnderline;
        loginCard.Controls.Add(forgotPasswordLink);

        Resize += (_, _) => CenterCard(loginCard);
        ResumeLayout();
    }

    private void CenterCard(Control loginCard)
    {
        loginCard.Left = (ClientSize.Width - loginCard.Width) / 2;
        loginCard.Top = (ClientSize.Height - loginCard.Height) / 2;
    }

    private Image? LoadLogoImage()
    {
        string logoPath = Path.Combine(Application.StartupPath, "Assets", "logo.png");

        if (!File.Exists(logoPath))
        {
            logoPath = Path.GetFullPath(Path.Combine(
                Application.StartupPath,
                "..",
                "..",
                "..",
                "Assets",
                "logo.png"));
        }

        return File.Exists(logoPath) ? Image.FromFile(logoPath) : null;
    }

    private Label CreateCenteredLabel(string text, Font font, Color color, int top, int height)
    {
        return new Label
        {
            Text = text,
            Font = font,
            ForeColor = color,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(760, height),
            Left = 10,
            Top = top
        };
    }

    private Label CreateFieldLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = TextColor,
            Size = new Size(620, 28),
            Left = left,
            Top = top
        };
    }

    private TextBox CreateInputTextBox(string placeholder, int left, int top)
    {
        RoundedInputPanel inputPanel = CreateInputPanel(left, top);
        TextBox textBox = CreateInnerTextBox(placeholder);
        inputPanel.Controls.Add(textBox);

        return textBox;
    }

    private RoundedInputPanel CreateInputPanel(int left, int top)
    {
        return new RoundedInputPanel
        {
            Size = new Size(620, 56),
            Left = left,
            Top = top,
            BackColor = Color.White,
            BorderColor = BorderColor,
            BorderRadius = 4
        };
    }

    private TextBox CreateInnerTextBox(string placeholder)
    {
        return new TextBox
        {
            Font = new Font("Segoe UI", 14F),
            PlaceholderText = placeholder,
            ForeColor = TextColor,
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            Left = 20,
            Top = 15,
            Width = 570,
            Height = 28
        };
    }

    private void TogglePasswordVisibility(object? sender, EventArgs e)
    {
        passwordHidden = !passwordHidden;
        passwordTextBox.UseSystemPasswordChar = passwordHidden;
        eyeButton.IsPasswordHidden = passwordHidden;
    }

    private void OpenDashboard(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(emailTextBox.Text) || string.IsNullOrWhiteSpace(passwordTextBox.Text))
        {
            MessageBox.Show("Email and password are required.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        UserAccount? user = authenticationService.Login(emailTextBox.Text, passwordTextBox.Text);

        if (user == null)
        {
            MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DashboardForm dashboardForm = new(appDataStore, user);
        dashboardForm.FormClosed += (_, _) => Close();
        dashboardForm.Show();
        Hide();
    }
}

public class RoundedPanel : Panel
{
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int BorderRadius { get; set; } = 12;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.FromArgb(220, 225, 235);

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color ShadowColor { get; set; } = Color.Transparent;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int ShadowSize { get; set; }

    public RoundedPanel()
    {
        DoubleBuffered = true;
        Padding = new Padding(0);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(Parent?.BackColor ?? Color.Transparent);

        Rectangle cardBounds = ClientRectangle;
        cardBounds.Inflate(-1, -1);
        cardBounds.Width -= 1;
        cardBounds.Height -= 1;

        if (ShadowSize > 0)
        {
            Rectangle shadowBaseBounds = cardBounds;
            shadowBaseBounds.Inflate(-ShadowSize / 2, -ShadowSize / 2);
            shadowBaseBounds.Offset(0, 6);

            for (int i = ShadowSize; i >= 1; i--)
            {
                int alpha = Math.Max(1, ShadowColor.A / (ShadowSize + 2));
                using Pen shadowPen = new(Color.FromArgb(alpha, ShadowColor), i);
                Rectangle shadowBounds = shadowBaseBounds;
                shadowBounds.Inflate(i / 2, i / 2);
                using GraphicsPath shadowPath = GetRoundedRectangle(shadowBounds, BorderRadius + 2);
                e.Graphics.DrawPath(shadowPen, shadowPath);
            }
        }

        using GraphicsPath path = GetRoundedRectangle(cardBounds, BorderRadius);
        using SolidBrush backgroundBrush = new(BackColor);
        e.Graphics.FillPath(backgroundBrush, path);

        using Pen borderPen = new(BorderColor, 1);
        e.Graphics.DrawPath(borderPen, path);

    }

    public static GraphicsPath GetRoundedRectangle(Rectangle rectangle, int radius)
    {
        GraphicsPath path = new();
        int diameter = radius * 2;

        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}

public class RoundedInputPanel : Panel
{
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int BorderRadius { get; set; } = 4;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.FromArgb(214, 221, 230);

    public RoundedInputPanel()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Rectangle bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;

        using GraphicsPath path = RoundedPanel.GetRoundedRectangle(bounds, BorderRadius);
        using SolidBrush backgroundBrush = new(BackColor);
        using Pen borderPen = new(BorderColor, 1);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(backgroundBrush, path);
        e.Graphics.DrawPath(borderPen, path);
    }
}

public class RoundedButton : Button
{
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int BorderRadius { get; set; } = 5;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        pevent.Graphics.Clear(Parent?.BackColor ?? Color.White);

        Rectangle bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;

        using GraphicsPath path = RoundedPanel.GetRoundedRectangle(bounds, BorderRadius);
        using SolidBrush backgroundBrush = new(BackColor);
        using SolidBrush textBrush = new(ForeColor);

        pevent.Graphics.FillPath(backgroundBrush, path);

        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            bounds,
            ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

public class EyeToggleButton : Button
{
    private bool isPasswordHidden = true;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool IsPasswordHidden
    {
        get => isPasswordHidden;
        set
        {
            isPasswordHidden = value;
            Invalidate();
        }
    }

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color IconColor { get; set; } = Color.FromArgb(64, 77, 97);

    public EyeToggleButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Text = string.Empty;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        pevent.Graphics.Clear(Parent?.BackColor ?? BackColor);

        RectangleF eyeBounds = new(7, 10, Width - 14, Height - 18);
        PointF center = new(Width / 2F, Height / 2F);

        using Pen iconPen = new(IconColor, 2F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using SolidBrush pupilBrush = new(IconColor);

        using GraphicsPath eyePath = new();
        eyePath.AddBezier(
            eyeBounds.Left,
            center.Y,
            eyeBounds.Left + 5,
            eyeBounds.Top,
            eyeBounds.Right - 5,
            eyeBounds.Top,
            eyeBounds.Right,
            center.Y);
        eyePath.AddBezier(
            eyeBounds.Right,
            center.Y,
            eyeBounds.Right - 5,
            eyeBounds.Bottom,
            eyeBounds.Left + 5,
            eyeBounds.Bottom,
            eyeBounds.Left,
            center.Y);

        pevent.Graphics.DrawPath(iconPen, eyePath);
        pevent.Graphics.FillEllipse(pupilBrush, center.X - 3.5F, center.Y - 3.5F, 7, 7);

        if (IsPasswordHidden)
        {
            pevent.Graphics.DrawLine(iconPen, eyeBounds.Left + 2, eyeBounds.Bottom + 2, eyeBounds.Right - 2, eyeBounds.Top - 2);
        }
    }
}

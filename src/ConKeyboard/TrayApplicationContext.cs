  using System;
using System.Windows.Forms;

namespace ConKeyboard;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private readonly KeyboardHook _hook;
    private bool _poweredOn;

    public TrayApplicationContext()
    {
        _hook = new KeyboardHook();

        _tray = new NotifyIcon { Visible = true, Text = "ConKeyboard" };
        _tray.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left) PoweredOn = !PoweredOn;
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Power On / Off", null, (s, e) => PoweredOn = !PoweredOn);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (s, e) => Exit());
        _tray.ContextMenuStrip = menu;

        PoweredOn = true;
        _tray.ShowBalloonTip(4000, "ConKeyboard is ON",
            "Hold TAB + letter = your alphabet\n" +
            "TAB + SHIFT + consonant = Kochi (¨)\n" +
            "TAB + / = ¿  (question starter)",
            ToolTipIcon.Info);
    }

    public bool PoweredOn
    {
        get => _poweredOn;
        set
        {
            _poweredOn = value;
            _hook.Enabled = value;
            _tray.Icon = IconFactory.Create(value);
            _tray.Text = value ? "ConKeyboard - ON" : "ConKeyboard - OFF";
        }
    }

    private void Exit()
    {
        _tray.Visible = false;
        _hook.Dispose();
        _tray.Dispose();
        ExitThread();
    }

    protected override void ExitThreadCore()
    {
        _tray.Visible = false;
        _hook.Dispose();
        base.ExitThreadCore();
    }
}

using System.Diagnostics;

public class PwmController
{
    private int gpioPin;

    public PwmController(int pin)
    {
        gpioPin = pin;
    }

    public void SetServoPulse(int pulseWidth)
    {
        Math.Clamp(pulseWidth, Program.IDLE_THRUSTER_PULSE_VALUE - Program.THRUSTER_MAX_PULSE_CHANGE, Program.IDLE_THRUSTER_PULSE_VALUE + Program.THRUSTER_MAX_PULSE_CHANGE);
        RunPgioCommand($"s {gpioPin} {pulseWidth}");
    }

    public void ResetServoPulse()
    {
        RunPgioCommand($"s {gpioPin} {Program.IDLE_THRUSTER_PULSE_VALUE}");
    }

    private void RunPgioCommand(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "pigs",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using (var process = Process.Start(psi))
        {
            if (process == null)
            {
                Console.WriteLine("Failed to start pigpio process");
                return;
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                Console.WriteLine($"pigpio error: {error}");
            }
        }
    }
}
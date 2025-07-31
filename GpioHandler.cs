using System.Device.Gpio;

public class GpioHandler : IDisposable
{
    private readonly GpioController controller;
    private readonly int pinNumber;

    public GpioHandler(int pinNumber)
    {
        this.pinNumber = pinNumber;
        controller = new GpioController();
        controller.OpenPin(this.pinNumber, PinMode.Output);
    }

    public void TurnOn()
    {
        controller.Write(pinNumber, PinValue.High);
    }

    public void TurnOff()
    {
        controller.Write(pinNumber, PinValue.Low);
    }

    public void Dispose()
    {
        controller.ClosePin(pinNumber);
        controller.Dispose();
    }
}
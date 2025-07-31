// Run "sudo pigpiod" in terminal before execution
public class Program
{
    public static readonly int IDLE_THRUSTER_PULSE_VALUE = 1500;

    //The max value it can increase or decrease from the idle value.
    public static readonly int THRUSTER_MAX_PULSE_CHANGE = 400;

    private static void Main()
    {
        JoystickHandler joystick = new JoystickHandler();
        PwmController leftThruster = new PwmController(17);
        PwmController rightThruster = new PwmController(18);
        PwmController verticalThruster = new PwmController(27);
        PwmController armMotor = new PwmController(24);
        PwmController clawMotor = new PwmController(23);
        GpioHandler killSwitch = new GpioHandler(8);

        //Make sure not to softlock
        killSwitch.TurnOff();

        //Make sure pins are in an an idle state
        List<PwmController> motors = new List<PwmController>();
        motors.Add(leftThruster);
        motors.Add(rightThruster);
        motors.Add(verticalThruster);
        motors.Add(armMotor);
        motors.Add(clawMotor);
        ResetPins(motors);

        while (true)
        {
            joystick.Update();

            //Check if we should terminate the program
            if (joystick.GetButtonState(9))
            {
                AhhhhhImOnFire(killSwitch);
                break;
            }
            if (joystick.GetButtonState(8))
            {
                ResetPins(motors);
                break;
            }

            //Apply power to forward motors
            float forwardAxisValue = -joystick.GetAxisValueNormalized(1);
            int leftThrusterPulse = AxisValueToServoPulse(forwardAxisValue);
            int rightThrusterPulse = AxisValueToServoPulse(forwardAxisValue);
            float horizontalValue = joystick.GetAxisValueNormalized(3);
            if (horizontalValue != 0)
            {
                int horizontalPulseValue = AxisValueToServoPulse(horizontalValue) - IDLE_THRUSTER_PULSE_VALUE;
                if (leftThrusterPulse != rightThrusterPulse)
                {
                    //How did this happen
                    rightThrusterPulse = leftThrusterPulse;
                }
                ApplyTurningTomotors(horizontalPulseValue, leftThrusterPulse, out leftThrusterPulse, out rightThrusterPulse);
            }
            leftThruster.SetServoPulse(leftThrusterPulse);
            rightThruster.SetServoPulse(rightThrusterPulse);

            //Apply power to vertical thruster
            float verticalAxisValue = joystick.GetAxisValueNormalized(5) / 2 - joystick.GetAxisValueNormalized(2) / 2;
            int verticalThrusterPulse = AxisValueToServoPulse(verticalAxisValue);
            verticalThruster.SetServoPulse(verticalThrusterPulse);

            //Check if it should activate the arm motors
            if (joystick.GetButtonState(7))
            {
                clawMotor.SetServoPulse(2000);
            } 
            else if(joystick.GetButtonState(4))
            {
                clawMotor.SetServoPulse(1000);
            }
            else
            {
                clawMotor.SetServoPulse(IDLE_THRUSTER_PULSE_VALUE);
            }

            if (joystick.GetButtonState(0))
            {
                armMotor.SetServoPulse(2000);
            }
            else if (joystick.GetButtonState(3))
            {
                armMotor.SetServoPulse(1000);
            }
            else
            {
                armMotor.SetServoPulse(IDLE_THRUSTER_PULSE_VALUE);
            }

            //Wait for .1 seconds before starting again

            Console.WriteLine($"Left Thruster: {leftThrusterPulse}");
            Console.WriteLine($"Right Thruster: {rightThrusterPulse}");
            Console.WriteLine($"Verticl Thruster: {verticalThrusterPulse}");
            Console.WriteLine("");
            Thread.Sleep(100);
        }

        joystick.CleanUp();
    }

    private static void ResetPins(List<PwmController> pwmControllerList)
    {
        foreach (PwmController controller in pwmControllerList)
        {
            controller.ResetServoPulse();
        }

    }

    private static int AxisValueToServoPulse(float axisValue)
    {
        int pulse = (int)(axisValue * THRUSTER_MAX_PULSE_CHANGE) + IDLE_THRUSTER_PULSE_VALUE;
        return Math.Clamp(pulse, IDLE_THRUSTER_PULSE_VALUE - THRUSTER_MAX_PULSE_CHANGE, IDLE_THRUSTER_PULSE_VALUE + THRUSTER_MAX_PULSE_CHANGE);
    }

    private static void ApplyTurningTomotors(int turnAmount, int thrusterPulseValue, out int leftThrusterPulseValue, out int rightThrusterPulseValue)
    {
        int maxThrusterPulseValue = IDLE_THRUSTER_PULSE_VALUE + THRUSTER_MAX_PULSE_CHANGE;
        if (turnAmount > 0)
        {
            //turning right
            int leftTurnAmount = thrusterPulseValue + turnAmount;
            if (leftTurnAmount > maxThrusterPulseValue)
            {
                int rightTurnAmount = thrusterPulseValue - (leftTurnAmount - maxThrusterPulseValue + turnAmount);
                leftTurnAmount = maxThrusterPulseValue;

                leftThrusterPulseValue = leftTurnAmount;
                rightThrusterPulseValue = rightTurnAmount;
            }
            else
            {
                int rightTurnAmount = thrusterPulseValue - turnAmount;

                leftThrusterPulseValue = leftTurnAmount;
                rightThrusterPulseValue = rightTurnAmount;
            }
        }
        else
        {
            //turning left
            int rightTurnAmount = thrusterPulseValue - turnAmount;
            if (rightTurnAmount > maxThrusterPulseValue)
            {
                int leftTurnAmount = thrusterPulseValue - (rightTurnAmount - maxThrusterPulseValue - turnAmount);
                rightTurnAmount = maxThrusterPulseValue;

                rightThrusterPulseValue = rightTurnAmount;
                leftThrusterPulseValue = leftTurnAmount;
            }
            else
            {
                int leftTurnAmount = thrusterPulseValue + turnAmount;

                rightThrusterPulseValue = rightTurnAmount;
                leftThrusterPulseValue = leftTurnAmount;
            }
        }
    }

    private static void AhhhhhImOnFire(GpioHandler killSwitch)
    {
        killSwitch.TurnOn();
    }
}
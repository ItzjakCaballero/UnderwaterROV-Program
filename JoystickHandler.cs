using static SDL2.SDL;

//Max value is +-32768
public class JoystickHandler
{
    private const int DEADZONE_VALUE = 4000;
    private const int MAX_JOYSTICK_VALUE = 32768;

    private IntPtr joystick;
    private int[] axisValues;
    private bool[] buttonStates;

    public JoystickHandler()
    {
        axisValues = new int[0];
        buttonStates = new bool[0];
        InitializeJoystick();
    }

    private void InitializeJoystick()
    {
        SDL_Init(SDL_INIT_JOYSTICK);
        int numberOfJoysticks = SDL_NumJoysticks();
        if (numberOfJoysticks < 1)
        {
            Console.WriteLine("No joysticks connected");
        }
        else
        {
            joystick = SDL_JoystickOpen(0);
            if (joystick == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to open joystick. SDL Error: {SDL_GetError()}");
            }
            else
            {
                Console.WriteLine($"Joysitck detected: {SDL_JoystickName(joystick)}");
            }
        }

        int numberOfAxis = SDL_JoystickNumAxes(joystick);
        axisValues = new int[numberOfAxis];

        int numberOfButtons = SDL_JoystickNumButtons(joystick);
        buttonStates = new bool[numberOfButtons];
    }

    public void Update()
    {
        SDL_JoystickUpdate();
        for(int i = 0; i < axisValues.Length; i++)
        {
            short newAxisValue = SDL_JoystickGetAxis(joystick, i);
            axisValues[i] = newAxisValue;
        }
        for(int i = 0; i < buttonStates.Length; i++)
        {
            byte state =  SDL_JoystickGetButton(joystick, i);
            buttonStates[i] = state == 1;
        }
    }

    public float GetAxisValueNormalized(int axis)
    {
        float returnValue = axisValues[axis];
        returnValue = ApplyDeadzoneToAxis((int)returnValue);
        returnValue = returnValue / MAX_JOYSTICK_VALUE;
        return returnValue;
    }

    public bool GetButtonState(int button)
    {
        return buttonStates[button];
    }

    private int ApplyDeadzoneToAxis(int axis)
    {
        if (axis >= 0)
        {
            if (axis < DEADZONE_VALUE)
            {
                axis = 0;
            }
            else if (axis > MAX_JOYSTICK_VALUE - DEADZONE_VALUE)
            {
                axis = MAX_JOYSTICK_VALUE;
            }
        }
        else
        {
            if (axis > -DEADZONE_VALUE)
            {
                axis = 0;
            }
            else if (axis < -MAX_JOYSTICK_VALUE + DEADZONE_VALUE)
            {
                axis = -MAX_JOYSTICK_VALUE;
            }
        }

        return axis;
    }

    public void CleanUp()
    {
        if(joystick != IntPtr.Zero)
        {
            SDL_JoystickClose(joystick);
        }
        SDL_Quit();
    }
}
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Button;
using Iot.Device.Uln2003;
using SimpleMaterialSystem;

namespace nf_threading_object_disposed_exception
{
    public class Program
    {
        private static bool _isWorking = false;
        private static Thread _workerThread;

        private static bool _requestCancellation = false;

        public const int STEPPER_MOTOR_BLUE_PIN = 21;
        public const int STEPPER_MOTOR_PINK_PIN = 20;
        public const int STEPPER_MOTOR_YELLOW_PIN = 19;
        public const int STEPPER_MOTOR_ORANGE_PIN = 18;

        public static void Main()
        {
            var button = new SimpleButton(10, pinMode: PinMode.InputPullDown);

            button.ButtonDown += (sender, e) =>
            {
                if (_isWorking)
                {
                    return;
                }

                _requestCancellation = false;
                _isWorking = true;

                Debug.WriteLine("Starting work...");

                if (_workerThread != null)
                {
                    _workerThread.Abort();
                    _workerThread = null;
                }

                _workerThread = new Thread(DoWork);
                _workerThread.Start();


                // This is just here to validate that the button is still working
                // AFTER the worker thread has been finished.
                // this is similar to my original setup where I have multiple threads
                var validationThread = new Thread(() =>
                {
                    _workerThread.Join();

                    button.ThisWillThrow();
                });
                validationThread.Start();
            };

            button.ButtonUp += (sender, e) =>
            {
                if (!_isWorking)
                {
                    return;
                }

                Debug.WriteLine("Cancelling work...");
                _requestCancellation = true;
            };

            Thread.Sleep(Timeout.Infinite);
        }

        private static void DoWork()
        {
            using Uln2003 motor = new Uln2003(
                STEPPER_MOTOR_BLUE_PIN,
                STEPPER_MOTOR_PINK_PIN,
                STEPPER_MOTOR_YELLOW_PIN,
                STEPPER_MOTOR_ORANGE_PIN
            );

            int totalSteps = 2048 / 8;
            int currentStep = 0;

            while (currentStep++ < totalSteps && !_requestCancellation)
            {
                motor.Mode = StepperMode.FullStepDualPhase;
                motor.RPM = 15;
                motor.Step(8);

                Debug.WriteLine($"Turn {currentStep}");
            }

            if (_requestCancellation)
            {
                Debug.WriteLine("Work cancelled.");
            }
            else
            {

                Debug.WriteLine("Work done.");
            }

            _requestCancellation = false;
            _isWorking = false;
        }
    }
}

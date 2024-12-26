using System;
using System.Device.Gpio;
using Iot.Device.Button;

namespace SimpleMaterialSystem
{
    class SimpleButton : ButtonBase
    {
        private readonly int _gpioPin;
        private readonly PinMode _pinMode;
        private GpioController controller;
        private GpioPin _pin;

        public SimpleButton(int gpioPin, PinMode pinMode = PinMode.InputPullDown)
            : base()
        {
            _gpioPin = gpioPin;
            _pinMode = pinMode;
            controller = new GpioController();

            IsPressed = true;

            Setup();
        }

        public void Setup()
        {
            if (_pin != null)
            {
                Reset();
            }

            _pin = controller.OpenPin(_gpioPin, _pinMode);
            _pin.ValueChanged += HandleValueChanged;
        }

        private void HandleValueChanged(object s, PinValueChangedEventArgs e)
        {
            if (e.ChangeType == PinEventTypes.Rising)
            {
                HandleButtonPressed();
            }
            else
            {
                HandleButtonReleased();
            }
        }

        public void Reset()
        {
            if (_pin != null)
            {
                controller.ClosePin(_gpioPin);

                _pin = null;
            }
        }

        public void ThisWillThrow()
        {
            // pin should still be working fine
            // but it is disposed
            // Who disposed it?
            var mode = _pin.GetPinMode();
            var v = _pin.Read();
        }
    }
}

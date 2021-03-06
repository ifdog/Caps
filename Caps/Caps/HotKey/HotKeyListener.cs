﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caps.HotKey.Structures;
using Caps.KeyBoard;
using Caps.KeyBoard.Structures;

namespace Caps.HotKey
{
	public class HotKeyListener:IDisposable
	{
		private readonly KeyHook _keyboardHook;
		public event EventHandler<HotKeyEventArgs> HotKeyTriggered;
		private ModifierKeyState _modifierKeyState;
		private bool _isSingleCaptial;
		private uint _lasttrig = 0;

		public HotKeyListener()
		{
			this._keyboardHook = new KeyHook(KeyboardEventCallback);
			this._keyboardHook.Hook();
		}

		private bool KeyboardEventCallback(int vkCode, KeyboardMessages keyboardMessage, uint time)
		{
			if (vkCode == VkCodes.VkCapital)
			{
				this._modifierKeyState.CapsLock = keyboardMessage == KeyboardMessages.WmKeydown;
				if (keyboardMessage == KeyboardMessages.WmKeyup && _isSingleCaptial)
				{
					Send.Key(VkCodes.VkCapital);
				}
				_isSingleCaptial = true;
				return false;
			}
			if (this._modifierKeyState.CapsLock)
			{
				_isSingleCaptial = false;
				switch (vkCode)
				{
					case VkCodes.VkLshift:
					case VkCodes.VkRshift:
						this._modifierKeyState.Shift = keyboardMessage == KeyboardMessages.WmKeydown;
						break;
					case VkCodes.VkLcontrol:
					case VkCodes.VkRcontrol:
						this._modifierKeyState.Ctrl = keyboardMessage == KeyboardMessages.WmKeydown;
						break;
					case VkCodes.VkLmenu:
					case VkCodes.VkRmenu:
						this._modifierKeyState.Alt = keyboardMessage == KeyboardMessages.WmSyskeydown;
						break;
					case VkCodes.VkLwin:
					case VkCodes.VkRwin:
						this._modifierKeyState.Win = keyboardMessage == KeyboardMessages.WmKeydown;
						break;
					default:
						if (keyboardMessage != KeyboardMessages.WmKeydown && keyboardMessage != KeyboardMessages.WmSyskeydown)
						{
							return true;
						}
						if (time - _lasttrig > 100)
						{
							HotKeyTriggered?.BeginInvoke(this,
								new HotKeyEventArgs(_modifierKeyState.Shift, _modifierKeyState.Ctrl, _modifierKeyState.Alt,
									_modifierKeyState.Win,
									vkCode), null, null);
							_lasttrig = time;
						}
						break;
				}
				return false;
			}
			return true;
		}

		public void Dispose()
		{
			this._keyboardHook.Unhook();
			this._keyboardHook.Dispose();
		}
	}
}

using System;
using System.Collections.Generic;
using CSCore.CoreAudioAPI;
using System.Linq;
using CSCore.Win32;

namespace Gouter.Managers
{
    /// <summary>
    /// 再生用サウンドデバイス状態の監視を行う
    /// </summary>
    internal class SoundDeviceListener : IDisposable, IMMNotificationClient
    {
        /// <summary>
        /// デバイスを状態変化通知を行うクラス
        /// </summary>
        private readonly MMDeviceEnumerator _deviceEnumerator;

        /// <summary>
        /// デバイスIDとデバイス情報の紐付け
        /// </summary>
        private readonly Dictionary<string, SoundDeviceInfo> _deviceInfosByDeviceId;

        /// <summary>
        /// システムのデフォルトデバイス情報
        /// </summary>
        public SoundDeviceInfo SystemDefault { get; }

        /// <summary>
        /// デバイス情報一覧
        /// </summary>
        public NotifiableCollection<SoundDeviceInfo> Devices { get; }

        /// <summary>
        /// デフォルトデバイス変更時に呼び出されるイベントハンドラ
        /// </summary>
        public event EventHandler<SoundDeviceInfo> DefaultDeviceChanged;

        /// <summary>
        /// デバイス接続時に呼び出されるイベントハンドラ
        /// </summary>
        public event EventHandler<SoundDeviceInfo> DeviceAdded;

        /// <summary>
        /// イベント接続解除時に呼び出されるイベントハンドラ
        /// </summary>
        public event EventHandler<SoundDeviceInfo> DeviceRemoved;

        /// <summary>
        /// デバイス状態変更時に呼び出されるイベントハンドラ
        /// </summary>
        public event EventHandler<SoundDeviceInfo> DeviceStateChanged;

        /// <summary>
        /// デバイス情報変更時に呼び出されるイベントハンドラ
        /// </summary>
        public event EventHandler<SoundDeviceInfo> PropertyValueChanged;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SoundDeviceListener()
        {
            var enumerator = new MMDeviceEnumerator();
            this._deviceEnumerator = enumerator;
            enumerator.RegisterEndpointNotificationCallback(this);

            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            this.SystemDefault = new SoundDeviceInfo(defaultDevice, true);

            this._deviceInfosByDeviceId = new Dictionary<string, SoundDeviceInfo>();

            var devices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

            this.Devices = new NotifiableCollection<SoundDeviceInfo> { this.SystemDefault };
            this.Devices.AddRange(devices.Select(device => new SoundDeviceInfo(device)));
        }

        /// <summary>デバイスIDからデバイス情報を取得する。</summary>
        /// <param name="deviceId">デバイスID</param>
        /// <returns>デバイス情報</returns>
        public SoundDeviceInfo this[string deviceId] => this._deviceInfosByDeviceId[deviceId];

        /// <summary>
        /// デバイス情報の有無を取得する。
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <returns>デバイス情報の有無</returns>
        public bool IsContains(string deviceId)
        {
            return this._deviceInfosByDeviceId.ContainsKey(deviceId);
        }

        /// <summary>
        /// デバイス一覧に追加する。
        /// </summary>
        /// <param name="device">デバイス情報</param>
        private SoundDeviceInfo AddDeviceImpl(MMDevice device)
        {
            var deviceInfo = new SoundDeviceInfo(device);

            this._deviceInfosByDeviceId.Add(deviceInfo.Id, deviceInfo);
            this.Devices.Add(deviceInfo);

            return deviceInfo;
        }

        /// <summary>
        /// デバイス一覧から削除する。
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        private void RemoveDeviceImpl(string deviceId)
        {
            if (!this.TryGet(deviceId, out var deviceInfo))
            {
                return;
            }

            this.Devices.Remove(deviceInfo);
            this._deviceInfosByDeviceId.Remove(deviceId);
        }

        /// <summary>
        /// デバイスIDからデバイス情報を取得する。
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="deviceInfo">デバイス情報</param>
        /// <returns>デバイス情報有無</returns>
        public bool TryGet(string deviceId, out SoundDeviceInfo deviceInfo)
        {
            return this._deviceInfosByDeviceId.TryGetValue(deviceId, out deviceInfo);
        }

        /// <summary>
        /// 監視対象のデバイスかどうかを返す。
        /// </summary>
        /// <param name="dataFlow">判定対象のDataFlow</param>
        /// <returns>対象か否か</returns>
        private static bool IsTargetDeviceDataFlow(DataFlow dataFlow)
        {
            return dataFlow == DataFlow.Render || dataFlow == DataFlow.All;
        }

        /// <summary>
        /// 監視対象のデバイス化どうかを返す。
        /// </summary>
        /// <param name="device">デバイス情報</param>
        /// <returns>監視対象か否か</returns>
        private static bool IsTargetDevice(MMDevice device)
        {
            return IsTargetDeviceDataFlow(device.DataFlow);
        }

        /// <summary>
        /// デバイス情報を取得する。
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="device">デバイス情報</param>
        /// <returns>デバイス情報</returns>
        private bool TryGetMMDevice(string deviceId, out MMDevice device)
        {
            device = this._deviceEnumerator.GetDevice(deviceId);
            return device != null;
        }

        /// <summary>
        /// デバイスの状態が変化した。
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="deviceState">デバイス状態</param>
        void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState deviceState)
        {
            if (this.TryGetMMDevice(deviceId, out var device))
            {
                return;
            }

            if (deviceState == DeviceState.Active)
            {
                // デバイスの状態がActiveに変化した
                this.AddDeviceImpl(device);
            }
            else
            {
                // デバイスの状態がActive以外に変化した
                this.RemoveDeviceImpl(device.DeviceID);
            }

            var deviceInfo = this[deviceId];
            this.DeviceStateChanged?.Invoke(this, deviceInfo);
        }

        /// <summary>
        /// デバイスの追加通知
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        void IMMNotificationClient.OnDeviceAdded(string deviceId)
        {
            if (!this.TryGetMMDevice(deviceId, out var device) || !IsTargetDevice(device))
            {
                return;
            }

            var deviceInfo = this.AddDeviceImpl(device);
            this.DeviceAdded?.Invoke(this, deviceInfo);
        }

        /// <summary>
        /// デバイスの削除通知
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
        {
            if (!this.TryGet(deviceId, out var deviceInfo))
            {
                return;
            }

            this.DeviceRemoved?.Invoke(this, deviceInfo);
            this.RemoveDeviceImpl(deviceId);
        }

        /// <summary>
        /// システムデバイスの変更通知
        /// </summary>
        /// <param name="dataFlow">DataFlow</param>
        /// <param name="role">Role</param>
        /// <param name="deviceId">デバイスID</param>
        void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow dataFlow, Role role, string deviceId)
        {
            if (!IsTargetDeviceDataFlow(dataFlow))
            {
                return;
            }

            var deviceInfo = this.SystemDefault;

            this.TryGetMMDevice(deviceId, out var device);
            deviceInfo.Update(device);

            this.DefaultDeviceChanged?.Invoke(this, deviceInfo);
        }

        /// <summary>
        /// デバイスのプロパティ変更通知
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="key">変更されたプロパティのキー</param>
        void IMMNotificationClient.OnPropertyValueChanged(string deviceId, PropertyKey key)
        {
            if (!this.TryGetMMDevice(deviceId, out var device) || !this.TryGet(deviceId, out var deviceInfo))
            {
                return;
            }

            deviceInfo.Update(device);
            this.PropertyValueChanged?.Invoke(this, deviceInfo);
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            var enumerator = this._deviceEnumerator;
            enumerator.UnregisterEndpointNotificationCallback(this);

            enumerator.Dispose();
        }
    }
}

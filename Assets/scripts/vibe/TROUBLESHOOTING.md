# Troubleshooting Guide - Unity 2D Pesawat Game

## 🐛 Common Issues dan Solutions

### 1. Input Tidak Berfungsi

**Problem**: Pesawat tidak bergerak dengan mouse atau gamepad
**Solutions**:
- Pastikan `PlayerInputManager` component sudah ditambahkan ke Player GameObject
- Check apakah `Enable Debug Log` dicentang untuk melihat input detection
- Verify bahwa gamepad/mouse terdeteksi di sistem
- Cek Console untuk error messages

**Debug Commands**:
```csharp
// Cek apakah device tersedia
playerInputManager.IsDeviceAvailable("gamepad");
playerInputManager.IsDeviceAvailable("mouse");

// Lihat info device aktif
Debug.Log(playerInputManager.GetActiveDeviceInfo());
```

### 2. Gamepad Tidak Override Mouse

**Problem**: Input mouse masih bekerja meskipun gamepad digunakan
**Solutions**:
- Turunkan `Gamepad Threshold` di PlayerInputManager (coba 0.05)
- Pastikan gamepad stick benar-benar bergerak
- Check `Input Switch Delay` - jangan terlalu tinggi
- Enable debug log untuk melihat input detection

### 3. Switching Input Terlalu Sering

**Problem**: Input terus-menerus beralih antara mouse dan gamepad
**Solutions**:
- Naikkan `Input Switch Delay` (coba 1.0 detik)
- Naikkan `Gamepad Threshold` untuk mengurangi noise
- Naikkan `Mouse Movement Threshold`
- Check apakah gamepad drift (stick bergerak sendiri)

### 4. Kamera Tidak Mengikuti

**Problem**: Kamera tidak bergerak atau tidak smooth
**Solutions**:
- Pastikan `CameraFollow` component sudah ada di Main Camera
- Set `Target` ke Player GameObject
- Check `Player Controller` reference
- Adjust `Smooth Speed` dan `Fast Follow Speed`

### 5. Speed Objects Tidak Muncul/Hilang

**Problem**: Objek tidak spawn/despawn berdasarkan kecepatan
**Solutions**:
- Verify `SpeedBasedObjectManager` memiliki reference ke `PlayerController`
- Check threshold values (Min/Max Speed)
- Pastikan `Target Object` sudah diassign
- Enable debug untuk melihat speed detection

### 6. Performance Issues

**Problem**: Game lag atau FPS drop
**Solutions**:
- Turunkan `Update Interval` di SpeedBasedObjectManager
- Disable debug logs di production
- Reduce particle count di boost effects
- Optimize SpeedBasedObjects list (hapus yang null)

## 🔧 Debug Tools

### PlayerController Debug:
```csharp
[ContextMenu("Show Player Stats")]
public void ShowPlayerStats()
{
    Debug.Log($"Current Speed: {CurrentSpeed}");
    Debug.Log($"Is Boosting: {IsBoosting}");
    Debug.Log($"Speed Percentage: {SpeedPercentage}");
}
```

### Input Manager Debug:
```csharp
// Tampilkan info input
Debug.Log($"Active Device: {playerInputManager.CurrentInputDevice}");
Debug.Log($"Aim Input: {playerInputManager.AimInput}");
Debug.Log($"Boost Input: {playerInputManager.BoostInput}");
```

### Camera Debug:
```csharp
// Check camera distance
Debug.Log($"Distance from Center: {cameraFollow.DistanceFromCenter}");
Debug.Log($"In Dead Zone: {cameraFollow.IsInDeadZone}");
```

## ⚙️ Performance Optimization

### 1. Update Frequencies
- **PlayerInputManager**: Update() - setiap frame (diperlukan untuk responsiveness)
- **SpeedBasedObjectManager**: 0.1s interval (bisa dinaikkan untuk performance)
- **CameraFollow**: LateUpdate() - setiap frame (diperlukan untuk smoothness)

### 2. Memory Management
```csharp
// Cleanup objects yang tidak digunakan
speedObjectManager.RemoveSpeedBasedObject(unusedObject);

// Disable debug logs di production
[SerializeField] private bool enableDebugLog = false; // Set ke false
```

### 3. Input Optimization
```csharp
// Kurangi frequency check untuk device detection
[SerializeField] private float deviceCheckInterval = 0.2f; // Instead of every frame
```

## 🎮 Controller Support

### Tested Controllers:
- ✅ Xbox One/Series Controller
- ✅ PlayStation 4/5 Controller
- ✅ Generic USB Gamepad
- ⚠️ Nintendo Switch Pro Controller (limited testing)

### Controller Setup:
1. Pastikan controller terdeteksi di Windows/Steam
2. Test dengan Input Debugger di Unity
3. Adjust `Gamepad Threshold` untuk sensitivitas yang tepat

## 📊 Performance Benchmarks

### Recommended Settings:
- **Gamepad Threshold**: 0.1 (balance antara responsiveness dan noise)
- **Mouse Movement Threshold**: 5 pixels
- **Input Switch Delay**: 0.5 seconds
- **Update Interval**: 0.1 seconds (SpeedBasedObjectManager)

### Performance Impact:
- **PlayerInputManager**: ~0.1ms per frame
- **PlayerController**: ~0.2ms per frame
- **CameraFollow**: ~0.1ms per frame
- **SpeedBasedObjectManager**: ~0.05ms per update interval

## 🚨 Known Issues

### 1. Unity Input System Package
- Memerlukan Input System Package 1.4.0 atau lebih baru
- Jika menggunakan Legacy Input, PlayerInputManager tidak akan berfungsi

### 2. Multiple Gamepads
- Saat ini hanya mendukung satu gamepad aktif
- Multiple gamepad support bisa ditambahkan jika diperlukan

### 3. Mobile Support
- Touch input belum diimplementasi
- Mobile gamepad support terbatas

## 💡 Tips Development

### 1. Testing Input:
```csharp
// Test dengan keyboard untuk simulasi gamepad
#if UNITY_EDITOR
if (Input.GetKey(KeyCode.W)) // Simulate gamepad up
{
    // Force gamepad input for testing
}
#endif
```

### 2. Build Considerations:
- Test di build sebenarnya, bukan hanya di editor
- Gamepad behavior bisa berbeda di build vs editor
- Test dengan multiple controller brands

### 3. Debug UI:
- Buat debug UI untuk menampilkan input status
- Helpful untuk testing dan troubleshooting
- Bisa disable di production build

## 📞 Support

Jika masih ada issues:
1. Check Unity Console untuk error messages
2. Enable debug logs di semua components
3. Test dengan controller yang berbeda
4. Verify Unity Input System package version
5. Check project Input Actions settings

Happy debugging! 🚀
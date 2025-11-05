# Unity 2D Pesawat Game - Setup Guide

## 📁 File yang Telah Dibuat

1. **PlayerController.cs** - Kontrol utama pesawat dengan fitur lengkap
2. **CameraFollow.cs** - Sistem kamera yang mengikuti pesawat
3. **SpeedBasedObjectManager.cs** - Manager untuk spawn/despawn objek berdasarkan kecepatan
4. **SmartInputManager.cs** - Manager prioritas input: gamepad vs mouse (simplified)
5. **GameManager.cs** - Contoh implementasi dan game logic
6. **InputExample.cs** - Contoh setup input manager
7. **Plane.cs** - Helper script untuk setup komponen (opsional)

## 🎮 Setup di Unity Editor

### 1. Setup Player GameObject

1. **Buat GameObject baru** untuk pesawat:
   - Nama: "Player" atau "Pesawat"  
   - Tambahkan komponen: `Rigidbody2D`, `SmartInputManager`, `PlayerController`
   - Set Rigidbody2D: `Gravity Scale = 0`, `Drag = 1`, `Angular Drag = 5`

2. **Setup SmartInputManager**:
   - **Settings**:
     - Gamepad Threshold: 0.1 (sensitivitas gamepad stick)
     - Mouse Threshold: 5 (sensitivitas mouse movement dalam pixel)
     - Gamepad Timeout: 2s (timeout untuk switch kembali ke mouse)
     - Debug Mode: ✓ (untuk melihat switching log)

3. **Setup PlayerController**:
   - **Movement Settings**:
     - Max Speed: 10
     - Min Speed: 2
     - Acceleration: 5
     - Movement Smoothness: 8
   - **Boost Settings**:
     - Boost Multiplier: 2
     - Boost Duration: 1
     - Boost Cooldown: 3
   - **Visual Feedback**:
     - Plane Visual: Assign child object dengan sprite pesawat
     - Boost Effect: Assign ParticleSystem untuk efek boost (opsional)

### 2. Setup Camera

1. **Setup Main Camera**:
   - Tambahkan komponen `CameraFollow`
   - Target: Assign player GameObject
   - Player Controller: Assign PlayerController component

2. **Camera Settings**:
   - **Follow Settings**:
     - Offset: (0, 0, -10)
     - Smooth Speed: 2
     - Fast Follow Speed: 8
   - **Dead Zone Settings**:
     - Dead Zone Radius: 2
     - Max Distance Before Fast Follow: 5

### 3. Setup Speed-Based Objects

1. **Buat GameObject** untuk speed-based manager:
   - Nama: "SpeedObjectManager"
   - Tambahkan komponen `SpeedBasedObjectManager`

2. **Setup Objects**:
   - Player Controller: Assign PlayerController
   - Speed Based Objects: Tambahkan objek yang ingin di-manage
   - Contoh setup:
     - **Slow Speed Object** (muncul saat lambat):
       - Target Object: GameObject yang ingin muncul saat lambat
       - Min Speed Threshold: 0
       - Max Speed Threshold: 5
       - Active When Below: ✓ (checked)
     - **Fast Speed Object** (muncul saat cepat):
       - Target Object: GameObject yang ingin muncul saat cepat
       - Min Speed Threshold: 7
       - Max Speed Threshold: 100
       - Active When Below: ✗ (unchecked)

## 🎯 Cara Penggunaan

### Input Controls:
- **Mouse**: Gerakkan mouse untuk mengarahkan pesawat (diabaikan jika gamepad aktif)
- **Gamepad**: Gunakan left stick untuk mengarahkan pesawat (prioritas utama)
- **Boost**: Klik kiri mouse atau R1 controller
- **Auto-Switch**: Input otomatis beralih berdasarkan device yang digunakan

### Fitur yang Tersedia:

1. **Smooth Movement**: Pesawat bergerak mengikuti arah input dengan smooth trajectory
2. **Speed System**: Kecepatan meningkat saat lurus, berkurang saat belok
3. **Boost System**: Kecepatan langsung naik saat boost dengan cooldown
4. **Smart Input**: Gamepad mengoveride mouse secara otomatis
5. **Adaptive Camera**: Kamera mengikuti dengan kecepatan yang adaptif
6. **Object Management**: Objek muncul/hilang berdasarkan kecepatan

## 🎮 Input System Prioritas

### Cara Kerja Input Manager:
1. **Gamepad Priority**: Ketika gamepad terdeteksi bergerak, input mouse diabaikan
2. **Auto Switch**: Sistem otomatis beralih ke mouse jika gamepad tidak digunakan selama 2 detik
3. **Smooth Transition**: Perpindahan input memiliki delay untuk mencegah switching yang terlalu sering
4. **Device Detection**: Sistem mendeteksi gerakan gamepad stick dan mouse movement secara real-time

### Setup Input Manager:
```csharp
// Subscribe ke event perubahan input device
playerInputManager.OnInputDeviceChanged += (isGamepad) => {
    Debug.Log($"Input switched to: {(isGamepad ? "Gamepad" : "Mouse")}");
};

// Force switch ke gamepad
playerInputManager.ForceGamepadMode();

// Force switch ke mouse
playerInputManager.ForceMouseMode();
```

## 🛠️ Customization

### Mengubah Kecepatan:
```csharp
playerController.SetMaxSpeed(15f); // Set kecepatan maksimum baru
```

### Mengaktifkan Boost Secara Manual:
```csharp
playerController.ForceBoost(); // Aktivasi boost tanpa input
```

### Menambah Object Speed-Based:
```csharp
speedObjectManager.AddSpeedBasedObject(myObject, 0f, 5f, true);
// Parameters: GameObject, minSpeed, maxSpeed, activeWhenBelow
```

### Mengubah Target Kamera:
```csharp
cameraFollow.SetTarget(newTarget.transform);
```

### Mengatur Input Sensitivity:
```csharp
// Set threshold gamepad (0.0 - 1.0)
playerInputManager.SetGamepadThreshold(0.2f);

// Set timeout gamepad inactivity
playerInputManager.SetGamepadTimeout(3f);
```

## 🐛 Debugging

### Debug Info:
- PlayerController menampilkan gizmos untuk arah gerakan
- CameraFollow menampilkan debug info di layar
- SpeedBasedObjectManager menampilkan status objek

### Console Commands:
- Right-click pada PlayerController → "Test Boost"
- Right-click pada PlayerController → "Reset Movement"

## 📝 Tips Penggunaan

1. **Performance**: SpeedBasedObjectManager menggunakan update interval untuk menghemat performance
2. **Visual Effects**: Tambahkan ParticleSystem untuk efek boost yang lebih menarik
3. **Audio**: Bisa ditambahkan AudioSource untuk suara boost dan gerakan
4. **Animation**: Gunakan Animator untuk animasi sprite pesawat
5. **Trail**: Tambahkan TrailRenderer untuk jejak pesawat

## 🎨 Contoh Hierarchy:

```
Player
├── PlayerInputManager (Script)
├── PlayerController (Script)
├── Visual (Sprite Renderer)
├── BoostEffect (Particle System)
└── TrailEffect (Trail Renderer)

Main Camera
└── CameraFollow (Script)

GameManager
├── SpeedObjectManager (Script)
├── InputExample (Script) - Optional
├── SlowSpeedObjects (Empty GameObject)
│   ├── Cloud1
│   ├── Cloud2
│   └── Background Elements
└── FastSpeedObjects (Empty GameObject)
    ├── SpeedLines
    ├── MotionBlur
    └── HighSpeed Effects

UI Canvas
├── InputDeviceText (Text)
├── InstructionText (Text)
└── SpeedSlider (Slider)
```

## 🚀 Next Steps

1. Tambahkan audio sistem untuk feedback suara
2. Implementasi power-ups dan obstacles
3. Tambahkan particle effects untuk visual yang lebih menarik
4. Buat sistem scoring berdasarkan kecepatan dan waktu
5. Implementasi multiple level dengan tantangan berbeda

Happy Flying! ✈️
全屏 CRT 效果 - 使用说明
========================

1. 在 Unity 中打开项目后，确认 Shaders/CRTEffect.shader 和 Materials/CRTEffect.mat 已正确导入。

2. 启用 CRT 效果：
   - 打开 Project 窗口，选中 Assets/Settings/Renderer2D.asset
   - 在 Inspector 中点击 "Add Renderer Feature"
   - 选择 "Full Screen Pass"
   - 将 Materials/CRTEffect 拖到 "Pass Material" 上
   - 勾选 "Fetch Color Buffer"（确保能读取当前画面）
   - Injection Point 建议选 "After Rendering Post Processing"（在后处理之后应用 CRT）

3. 调节参数（在材质 CRTEffect 的 Inspector 中）：
   - Scanline Intensity：扫描线强度 (0~1)
   - Scanline Count：扫描线密度
   - Vignette Strength：暗角强度
   - Vignette Softness：暗角过渡柔和度
   - Screen Curvature：屏幕弯曲程度（0 为关闭）
   - Chromatic Aberration：色差强度（0 为关闭）

4. 若不需要某种效果，可将对应强度/数值设为 0。

using System;
using UnityEngine;
public class MinimumTimeAcceleration
{
    private const float Tolerance = 1e-6f;
    private const int MaxIterations = 100;

    public static Vector2 AccelerationDirection(Vector2 v, float a, float x, float y)
    {
        // 处理加速度为零的情况
        if (a <= Tolerance)
        {
            // 匀速直线运动：只有点p在速度方向上才可达
            float vMagSq = v.sqrMagnitude;
            if (vMagSq <= Tolerance * Tolerance)
                return new Vector2(1, 0); // 默认方向

            float dot = x * v.x + y * v.y;
            if (dot > 0 && Math.Abs(x * v.y - y * v.x) < Tolerance)
                return new Vector2(1, 0); // 可达，但方向任意

            return new Vector2(1, 0); // 不可达时返回默认方向
        }

        // 计算基本参数
        float R = MathF.Sqrt(x * x + y * y);
        float vMag = v.magnitude;
        float D = x * v.x + y * v.y;

        // 处理目标点在原点的情况
        if (R <= Tolerance)
        {
            if (vMag > Tolerance)
            {
                // 返回初速度的反方向
                return new Vector2(-v.x / vMag, -v.y / vMag);
            }
            return new Vector2(1, 0); // 初速也为零时返回默认方向
        }

        // 求解最小时间T的四次方程
        float T = SolveMinTime(a, vMag, D, R);

        // 计算加速度矢量
        float axVec = 2 * (x - v.x * T) / (T * T);
        float ayVec = 2 * (y - v.y * T) / (T * T);

        // 归一化为单位向量
        float mag = MathF.Sqrt(axVec * axVec + ayVec * ayVec);
        if (mag > Tolerance)
        {
            return new Vector2(axVec / mag, ayVec / mag);
        }

        // 零向量保护（理论上不会发生）
        return new Vector2(1, 0);
    }

    private static float SolveMinTime(float a, float vMag, float D, float R)
    {
        // 计算牛顿迭代初始值
        float T = vMag > Tolerance
            ? (-vMag + MathF.Sqrt(vMag * vMag + 2 * a * R)) / a
            : MathF.Sqrt(2 * R / a);

        // 牛顿迭代求解四次方程
        for (int i = 0; i < MaxIterations; i++)
        {
            float T2 = T * T;
            float T3 = T2 * T;
            float T4 = T3 * T;

            // 计算函数值 f(T) = a²T⁴ - 4v²T² + 8DT - 4R²
            float f = a * a * T4 - 4 * vMag * vMag * T2 + 8 * D * T - 4 * R * R;

            // 计算导数值 f'(T) = 4a²T³ - 8v²T + 8D
            float df = 4 * a * a * T3 - 8 * vMag * vMag * T + 8 * D;

            if (MathF.Abs(df) < Tolerance) break;

            float delta = f / df;
            T -= delta;

            if (T <= 0) T = 0.1f; // 保持正值
            if (MathF.Abs(delta) < Tolerance) break;
        }

        return T;
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARED_TOOLS.SCENARIO
{
    public static class RotationUtils
    {
        //pos = x, y, z

        public static float[] RotationInX(float[] pos, float angleInRadian)
        {
            float[] res = new float[3];
            res[0] = pos[0];
            res[1] = (float)(Math.Cos(angleInRadian) * pos[1] - Math.Sin(angleInRadian) * pos[2]);
            res[2] = (float)(Math.Sin(angleInRadian) * pos[1] + Math.Cos(angleInRadian) * pos[2]);
            return res;
        }

        public static float[] RotationInY(float[] pos, float angleInRadian)
        {
            float[] res = new float[3];
            res[0] = (float)(Math.Cos(angleInRadian) * pos[0] + Math.Sin(angleInRadian) * pos[2]);
            res[1] = pos[1];
            res[2] = (float)(-1 * Math.Sin(angleInRadian) * pos[0] + Math.Cos(angleInRadian) * pos[2]);
            return res;
        }

        public static float[] RotationInZ(float[] pos, float angleInRadian)
        {
            float[] res = new float[3];
            res[0] = (float)(Math.Cos(angleInRadian) * pos[0] - Math.Sin(angleInRadian) * pos[1]);
            res[1] = (float)(Math.Sin(angleInRadian) * pos[0] + Math.Cos(angleInRadian) * pos[1]);
            res[2] = pos[2];
            return res;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Correct
{
    class Filter
    {


        public const int Order = 6; //6阶
        private float[] Xi=new float[Order+1];
        private float[] Yi = new float[Order];
        int filterIndex = -1;
        private static readonly float[][] A = new float[][] { 
             
                new float[]{1,   -4.787135601f,    9.649518013f,   -10.46907902f,    6.441112041f,     -2.129038811f,   0.2951724231f}, //400低通
                new float[]{ 1,  -5.310338974f,     12.2460556f,   -15.62362957f,    11.62108135f,     -4.782167912f,   0.8546014428f}, //550
                new float[]{1,   -5.101892948f,     11.5223484f,   -14.59983158f,    10.93431854f,     -4.594454765f,   0.8546014428f},//650
                new float[]{1,   -4.861992359f,    10.72529697f,   -13.48244953f,    10.17795658f,     -4.378414631f,   0.8546014428f},//750
                new float[]{1,   -4.592116356f,    9.874526978f,   -12.29969311f,    9.370617867f,     -4.135380745f,   0.8546014428f}, //850

                new float[]{1,   -1.270195723f,    2.914719343f,   -2.111337185f,    2.366715431f,   -0.8332899213f,   0.5320753455f}, //1700
                new float[]{1,-6.621495835e-16f,    2.374094725f,-1.356818985e-15f,    1.929355621f, -6.430251148e-16f,   0.5320753455f}, //2000
                new float[]{1,    1.270195723f,    2.914719343f,    2.111337185f,    2.366715431f,    0.8332899213f,   0.5320753455f}, //2300
                new float[]{ 1,      2.4702003f,    4.418744087f,     4.52286768f,    3.583455563f,     1.620532036f,   0.5320753455f}, //2600
            };

        private static readonly float[][] B = new float[][] { 

                new float[]{8.576556866e-06f,5.145934119e-05f,0.0001286483603f,0.0001715311373f,0.0001286483603f, 5.145934119e-05f,8.576556866e-06f},

                new float[]{5.607011553e-05f,              0,-0.0001682103466f,              0,0.0001682103466f,                0,-5.607011553e-05f},
                new float[]{5.607011553e-05f,              0,-0.0001682103466f,              0,0.0001682103466f,                0,-5.607011553e-05f},
                new float[]{5.607011553e-05f,              0,-0.0001682103466f,              0,0.0001682103466f,                0,-5.607011553e-05f},
                new float[]{5.607011553e-05f,              0,-0.0001682103466f,              0,0.0001682103466f,                0,-5.607011553e-05f},

                new float[]{0.002898194594f, 0, -0.00869458355f,  0,  0.00869458355f,  0,-0.002898194594f},
                new float[]{0.002898194594f, 0, -0.00869458355f,  0,  0.00869458355f,  0,-0.002898194594f},
                new float[]{0.002898194594f, 0, -0.00869458355f,  0,  0.00869458355f,  0,-0.002898194594f},
                new float[]{0.002898194594f, 0, -0.00869458355f,  0,  0.00869458355f,  0,-0.002898194594f},
            };




        public void SetFilterIndex(int index)
        {
            this.filterIndex = index;
        }

        public float DoFilter(float ad)
        {
            if (filterIndex < 0) return ad;


            float y = 0;

            for (int i = 0; i < Order; i++)
            {
                Xi[i] = Xi[1 + i];
            }
            Xi[Order] = ad;

            for (int j = 0; j <= Order; j++)
            {
                y += B[filterIndex][j] * Xi[j];
                if (j > 0)
                {
                    y -= A[filterIndex][j] * Yi[Order - j];
                }
            }

            for (int i = 0; i < Order-1; i++)
            {
                Yi[i] = Yi[1 + i];
            }
            Yi[Order - 1] = y;
            

            return y;
        }

    }
}

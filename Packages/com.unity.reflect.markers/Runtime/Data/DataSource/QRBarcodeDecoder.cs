using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Reflect.Markers.Camera
{
    public sealed class QRBarcodeDecoder : IBarcodeDecoder
    {
        byte[] m_Colors32;
        int m_Width;
        int m_Height;
        bool m_Disposed;
        object m_ColorsLock = new object();
        Thread m_DecodeThread;
        Thread m_ConvertThread;
        public async Task<string> Decode(Color32Result image)
        {
            RecycleByteArray();
            m_Width = (int)image.Size.x;
            m_Height = (int)image.Size.y;
            await ConvertBytes(image.ColorArr);
            ZXing.Result result = await DecodeThread();
            if (result != null)
                return result.Text;
            return null;
        }

        async Task<ZXing.Result> DecodeThread()
        {
            ZXing.BarcodeReader barcodeReader = new ZXing.BarcodeReader();
            ZXing.Result res = null;
            bool running = true;

            void DecodingThreadFunc()
            {
                lock (m_ColorsLock)
                {
                    if (m_Colors32 != null && m_Colors32.Length > 0)
                        res = barcodeReader.Decode(m_Colors32, m_Width, m_Height, ZXing.RGBLuminanceSource.BitmapFormat.RGBA32);
                }
                running = false;
            }
            m_DecodeThread = new Thread(DecodingThreadFunc);
            m_DecodeThread.Start();

            while (running)
                await Task.Delay(1);

            return res;
        }

        async Task ConvertBytes(Color32[] colors)
        {
            bool running = true;

            void Convert()
            {
                if (colors != null)
                {
                    lock (m_ColorsLock)
                    {
                        if (m_Colors32 == null || m_Colors32.Length != colors.Length * 4)
                            m_Colors32 = new byte[colors.Length * 4];

                        for (int i = 0; i < colors.Length; i++)
                        {
                            int start = i * 4;

                            m_Colors32[start] = colors[i].r;
                            m_Colors32[start + 1] = colors[i].g;
                            m_Colors32[start + 2] = colors[i].b;
                            m_Colors32[start + 3] = colors[i].a;
                        }
                    }
                }

                running = false;
            }
            m_ConvertThread = new Thread(Convert);
            m_ConvertThread.Start();

            while (running)
                await Task.Delay(1);
        }

        void RecycleByteArray()
        {
            lock (m_ColorsLock)
            {
                if (m_Colors32 != null)
                {
                    Array.Clear(m_Colors32, 0, m_Colors32.Length);
                }
            }
        }

        ~QRBarcodeDecoder()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;
            if (disposing)
            {
                lock (m_ColorsLock)
                {
                    m_Colors32 = null;
                }
            }
            // Close threads
            ThreadCloser(m_ConvertThread);
            ThreadCloser(m_DecodeThread);
            m_Disposed = true;
        }

        static void ThreadCloser(Thread thread)
        {
            if (thread != null && thread.ThreadState != ThreadState.Stopped)
            {
                thread.Abort();
                thread.Join();
            }
        }

    }

}

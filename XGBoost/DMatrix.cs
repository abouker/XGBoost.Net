﻿using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace XGBoost
{
  public class DMatrix : IDisposable
  {
    private DMatrixHandle _handle;
    private float Missing = -1.0F; // arbitrary value used to represent a missing value
    
    public DMatrixHandle Handle
    {
      get { return _handle; }
    }

    public float[] Label
    {
      get { return GetFloatInfo("label"); }
      set { SetFloatInfo("label", value); }
    }

    public DMatrix(float[][] data, float[] labels = null)
    {
      float[] data1D = Flatten2DArray(data);
      ulong nrows = unchecked((ulong)data.Length);
      ulong ncols = unchecked((ulong)data[0].Length);
      int output = DllMethods.XGDMatrixCreateFromMat(data1D, nrows, ncols, Missing, out _handle);
      if (output == -1) 
        throw new DllFailException("XGDMatrixCreateFromMat() in DMatrix() failed");

      if (labels != null)
      {
        Label = labels;
      }
    }

    private float[] Flatten2DArray(float[][] data2D)
    {
      int elementsNo = 0;
      for (int row = 0; row < data2D.Length; row++)
      {
        elementsNo += data2D[row].Length;
      }

      float[] data1D = new float[elementsNo];
      int ind = 0;
      for (int row = 0; row < data2D.Length; row++)
      {
        for (int col = 0; col < data2D[row].Length; col++)
        {
          data1D[ind] = data2D[row][col];
        }
      }
      return data1D;
    }

    private float[] GetFloatInfo(string field)
    {
      ulong lenULong;
      IntPtr result;
      int output = DllMethods.XGDMatrixGetFloatInfo(_handle, field, out lenULong, out result);
      if (output == -1)
        throw new DllFailException("XGDMatrixGetFloatInfo() in DMatrix.GetFloatInfo() failed");

      int len = unchecked((int)lenULong);
      float[] floatInfo = new float[len];
      for (int i = 0; i < len; i++)
      {
        byte[] floatBytes = new byte[4];
        floatBytes[0] = Marshal.ReadByte(result, 4 * i + 0);
        floatBytes[1] = Marshal.ReadByte(result, 4 * i + 1);
        floatBytes[2] = Marshal.ReadByte(result, 4 * i + 2);
        floatBytes[3] = Marshal.ReadByte(result, 4 * i + 3);
        float f = BitConverter.ToSingle(floatBytes, 0);
        floatInfo[i] = f;
      }
      return floatInfo;
    }

    private void SetFloatInfo(string field, float[] floatInfo)
    {
      ulong len = (ulong)floatInfo.Length;
      int output = DllMethods.XGDMatrixSetFloatInfo(_handle, field, floatInfo, len);
      if (output == -1)
        throw new DllFailException("XGDMatrixSetFloatInfo() in DMatrix.SetFloatInfo() failed");
    }

    public void Dispose()
    {
      if (_handle != null && !_handle.IsInvalid)
      {
        _handle.Dispose();
      }
    }
  }

  public class DMatrixHandle : SafeHandleZeroOrMinusOneIsInvalid
  {
    private DMatrixHandle()
        : base(true)
    {
    }

    override protected bool ReleaseHandle()
    {
      int output = DllMethods.XGDMatrixFree(handle);
      return output == 0 ? true : false;
    }
  }
}

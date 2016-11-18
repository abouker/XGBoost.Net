﻿using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace XGBoost
{
  public class DMatrix : IDisposable
  {
    private DMatrixHandle _handle;

    public float[] BaseMargin
    {
      get { return GetFloatInfo("base_margin"); }
    }

    public float[] Label
    {
      get { return GetFloatInfo("label"); }
    }

    public float[] Weight
    {
      get { return GetFloatInfo("weight"); }
    }

    public DMatrix(string dataPath, bool silent = false)
    {
      int output = DllMethods.XGDMatrixCreateFromFile(dataPath, silent ? 1 : 0, out _handle);
      if (output == -1) 
        throw new DllFailException("XGDMatrixCreateFromFile() in DMatrix() failed");
    }

    public string[] FeatureNames()
    {
      return null; 
    }

    public string[] FeatureTypes()
    {
      return null;
    }

    public float[] GetFloatInfo(string field)
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
        floatBytes[0] = Marshal.ReadByte(result, 4*i + 0);
        floatBytes[1] = Marshal.ReadByte(result, 4*i + 1);
        floatBytes[2] = Marshal.ReadByte(result, 4*i + 2);
        floatBytes[3] = Marshal.ReadByte(result, 4*i + 3);
        float f = System.BitConverter.ToSingle(floatBytes, 0);
        floatInfo[i] = f;
      }
      return floatInfo;
    }

    public int NumCol()
    {
      ulong colsULong;
      int output = DllMethods.XGDMatrixNumCol(_handle, out colsULong);
      if (output == -1) 
        throw new DllFailException("XGDMatrixNumCol() in DMatrix.NumCol() failed");
      int cols = unchecked((int) colsULong);
      return cols;
    }

    public int NumRow()
    {
      ulong rowsULong;
      int output = DllMethods.XGDMatrixNumRow(_handle, out rowsULong);
      if (output == -1) 
        throw new DllFailException("XGDMatrixNumRow() in DMatrix.NumRow() failed");
      int rows = unchecked((int)rowsULong);
      return rows;
    }

    public void SaveBinary()
    {
    }

    public void SetBaseMargin()
    {
    }

    public void SetFloatInfo(string field, float[] floatInfo)
    {
      ulong len = (ulong)floatInfo.Length;
      int output = DllMethods.XGDMatrixSetFloatInfo(_handle, field, floatInfo, len);
      if (output == -1)
        throw new DllFailException("XGDMatrixSetFloatInfo() in DMatrix.SetFloatInfo() failed");
    }

    public void SetGroup()
    {
    }

    public void SetLabel()
    {
    }

    public void SetWeight()
    {
    }

    public DMatrix Slice()
    {
      return null;
    }

    public void Dispose()
    {
      if (_handle != null && !_handle.IsInvalid)
      {
        _handle.Dispose();
      }
    }
  }

  internal class DMatrixHandle : SafeHandleZeroOrMinusOneIsInvalid
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

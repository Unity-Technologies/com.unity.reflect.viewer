//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class vx_evt_tts_injection_ended_t : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal vx_evt_tts_injection_ended_t(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(vx_evt_tts_injection_ended_t obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~vx_evt_tts_injection_ended_t() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          VivoxCoreInstancePINVOKE.delete_vx_evt_tts_injection_ended_t(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public vx_evt_base_t base_ {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_tts_injection_ended_t_base__set(swigCPtr, vx_evt_base_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = VivoxCoreInstancePINVOKE.vx_evt_tts_injection_ended_t_base__get(swigCPtr);
      vx_evt_base_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new vx_evt_base_t(cPtr, false);
      return ret;
    } 
  }

  public uint num_consumers {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_tts_injection_ended_t_num_consumers_set(swigCPtr, value);
    } 
    get {
      uint ret = VivoxCoreInstancePINVOKE.vx_evt_tts_injection_ended_t_num_consumers_get(swigCPtr);
      return ret;
    } 
  }

  public uint utterance_id {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_tts_injection_ended_t_utterance_id_set(swigCPtr, value);
    } 
    get {
      uint ret = VivoxCoreInstancePINVOKE.vx_evt_tts_injection_ended_t_utterance_id_get(swigCPtr);
      return ret;
    } 
  }

  public vx_tts_destination tts_destination {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_tts_injection_ended_t_tts_destination_set(swigCPtr, (int)value);
    } 
    get {
      vx_tts_destination ret = (vx_tts_destination)VivoxCoreInstancePINVOKE.vx_evt_tts_injection_ended_t_tts_destination_get(swigCPtr);
      return ret;
    } 
  }

  public vx_evt_tts_injection_ended_t() : this(VivoxCoreInstancePINVOKE.new_vx_evt_tts_injection_ended_t(), true) {
  }

}

//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.12
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class vx_evt_sessiongroup_updated_t : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal vx_evt_sessiongroup_updated_t(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(vx_evt_sessiongroup_updated_t obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~vx_evt_sessiongroup_updated_t() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          VivoxCoreInstancePINVOKE.delete_vx_evt_sessiongroup_updated_t(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public vx_evt_base_t base_ {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_base__set(swigCPtr, vx_evt_base_t.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_base__get(swigCPtr);
      vx_evt_base_t ret = (cPtr == global::System.IntPtr.Zero) ? null : new vx_evt_base_t(cPtr, false);
      return ret;
    } 
  }

  public string sessiongroup_handle {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_sessiongroup_handle_set(swigCPtr, value);
    } 
    get {
      string ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_sessiongroup_handle_get(swigCPtr);
      return ret;
    } 
  }

  public int in_delayed_playback {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_in_delayed_playback_set(swigCPtr, value);
    } 
    get {
      int ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_in_delayed_playback_get(swigCPtr);
      return ret;
    } 
  }

  public double current_playback_speed {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_current_playback_speed_set(swigCPtr, value);
    } 
    get {
      double ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_current_playback_speed_get(swigCPtr);
      return ret;
    } 
  }

  public vx_sessiongroup_playback_mode current_playback_mode {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_current_playback_mode_set(swigCPtr, (int)value);
    } 
    get {
      vx_sessiongroup_playback_mode ret = (vx_sessiongroup_playback_mode)VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_current_playback_mode_get(swigCPtr);
      return ret;
    } 
  }

  public int playback_paused {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_playback_paused_set(swigCPtr, value);
    } 
    get {
      int ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_playback_paused_get(swigCPtr);
      return ret;
    } 
  }

  public int loop_buffer_capacity {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_loop_buffer_capacity_set(swigCPtr, value);
    } 
    get {
      int ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_loop_buffer_capacity_get(swigCPtr);
      return ret;
    } 
  }

  public int first_loop_frame {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_first_loop_frame_set(swigCPtr, value);
    } 
    get {
      int ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_first_loop_frame_get(swigCPtr);
      return ret;
    } 
  }

  public int total_loop_frames_captured {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_total_loop_frames_captured_set(swigCPtr, value);
    } 
    get {
      int ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_total_loop_frames_captured_get(swigCPtr);
      return ret;
    } 
  }

  public int last_loop_frame_played {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_last_loop_frame_played_set(swigCPtr, value);
    } 
    get {
      int ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_last_loop_frame_played_get(swigCPtr);
      return ret;
    } 
  }

  public string current_recording_filename {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_current_recording_filename_set(swigCPtr, value);
    } 
    get {
      string ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_current_recording_filename_get(swigCPtr);
      return ret;
    } 
  }

  public int total_recorded_frames {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_total_recorded_frames_set(swigCPtr, value);
    } 
    get {
      int ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_total_recorded_frames_get(swigCPtr);
      return ret;
    } 
  }

  public long first_frame_timestamp_us {
    set {
      VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_first_frame_timestamp_us_set(swigCPtr, value);
    } 
    get {
      long ret = VivoxCoreInstancePINVOKE.vx_evt_sessiongroup_updated_t_first_frame_timestamp_us_get(swigCPtr);
      return ret;
    } 
  }

  public vx_evt_sessiongroup_updated_t() : this(VivoxCoreInstancePINVOKE.new_vx_evt_sessiongroup_updated_t(), true) {
  }

}

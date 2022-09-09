using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

namespace HoloFab {
	namespace CustomData {
		public class TaskInterface {
			public string sourceName = "Task Intrface";
            
			// Task Object References.
			private CancellationTokenSource cancellation;
			private Task task;
            
			// History:
			// - Debug History.
			public List<string> debugMessages = new List<string>();
			// Actual Action to be ran in the loop to be overridden.
			public Action taskAction;
			// Action Type to check if loop should break.
			public delegate bool LoopConditionCheck();
			// Actiual Action for loop checking to be overriden.
			public LoopConditionCheck checkCondition;
			// delay between each loop execution in milliseconds
			public int delayInTask;
			// Identifier of the task.
			public string taskName;
            
			public TaskInterface(Action _taskAction, int _delayInTask=0, LoopConditionCheck _checkCondition=null, string _taskName="") {
				this.taskAction = _taskAction;
				this.delayInTask = _delayInTask;
				if (_checkCondition == null)
					_checkCondition = CheckLoopCondition;
				this.checkCondition = _checkCondition;
				this.taskName = _taskName;
			}
            
			//////////////////////////////////////////////////////////////////////////
            
			public void Start() {
				if ((this.taskAction != null) && (this.task == null)) {
					this.debugMessages = new List<string>();
					// Start the thread.
					this.cancellation = new CancellationTokenSource();
					this.task = Task.Run(() => {
						ThreadLoop();
					}, this.cancellation.Token);
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Task ["+this.taskName+"] Started.", ref this.debugMessages);
					#endif
				}
			}
			public void Stop() {
				// Reset.
				if (this.task != null) {
					this.cancellation.Cancel();
					this.task.Wait(2);
					this.cancellation.Dispose();
					this.task = null;     // Good Practice?
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Stopping Task ["+this.taskName+"].", ref this.debugMessages);
					#endif
				}
			}
            
			// Default Check to run on Loop - infinite loop
			public static bool CheckLoopCondition() {
				return true;
			}
            
			// Infinite Loop Executing set function.
			public void ThreadLoop() {
				if (this.taskAction != null) {
					while ((this.task != null) && this.checkCondition()) {
						this.taskAction();
						this.task?.Wait(this.delayInTask);
					}
				}
			}
		}
	}
}
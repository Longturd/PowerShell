// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// Class for Get-Error implementation.
    /// </summary>
    [Experimental("Microsoft.PowerShell.Utility.PSGetError", ExperimentAction.Show)]
    [Cmdlet(VerbsCommon.Get, "Error",
        HelpUri = "https://docs.microsoft.com/powershell/module/microsoft.powershell.utility/get-error?view=powershell-7&WT.mc_id=ps-gethelp",
        DefaultParameterSetName = NewestParameterSetName)]
    public sealed class GetErrorCommand : PSCmdlet
    {
        internal const string ErrorParameterSetName = "Error";
        internal const string NewestParameterSetName = "Newest";
        internal const string AliasNewest = "Last";

        /// <summary>
        /// Gets or sets the error object to resolve.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = ErrorParameterSetName)]
        [ValidateNotNullOrEmpty]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// Gets or sets the number of error objects to resolve starting with newest first.
        /// </summary>
        [Parameter(ParameterSetName = NewestParameterSetName)]
        [Alias(AliasNewest)]
        [ValidateRange(1, int.MaxValue)]
        public int Newest { get; set; } = 1;

        /// <summary>
        /// Process the error object.
        /// </summary>
        protected override void ProcessRecord()
        {
            var errorRecords = new List<object>();
            var index = 0;

            if (InputObject != null)
            {
                if (InputObject.BaseObject is Exception || InputObject.BaseObject is ErrorRecord)
                {
                    errorRecords.Add(InputObject);
                }
            }
            else
            {
                var errorVariable = SessionState.PSVariable.Get("error");
                var count = Newest;
                ArrayList errors = (ArrayList)errorVariable.Value;
                if (count > errors.Count)
                {
                    count = errors.Count;
                }

                while (count > 0)
                {
                    errorRecords.Add(errors[index]);
                    index++;
                    count--;
                }
            }

            index = 0;
            bool addErrorIdentifier = errorRecords.Count > 1 ? true : false;

            foreach (object errorRecord in errorRecords)
            {
                PSObject obj = PSObject.AsPSObject(errorRecord);
                obj.TypeNames.Insert(0, "PSExtendedError");

                // Remove some types so they don't get rendered by those formats
                obj.TypeNames.Remove("System.Management.Automation.ErrorRecord");
                obj.TypeNames.Remove("System.Exception");

                if (addErrorIdentifier)
                {
                    obj.Properties.Add(new PSNoteProperty("PSErrorIndex", index++));
                }

                WriteObject(obj);
            }
        }
    }
}

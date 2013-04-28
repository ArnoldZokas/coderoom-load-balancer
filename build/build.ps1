framework "4.0"

. .\functions.ps1

properties {
	$build_dir = split-path $psake.build_script_file
	$project_dir = "$build_dir\.."
	
	$solution_file = "$project_dir\Coderoom.LoadBalancer.sln"
	$build_configuration = "Release"
	$build_output_dir = "$project_dir\build-output"
}

task default -depends compile

formatTaskName {
	param($taskName)
	write-host
	write-host (("-"*30) + "[ " + $taskName.ToUpper() + " ]" + ("-"*30)) -fore Green
	write-host
}

taskSetup {
	$message = "Running task $($psake.context.Peek().currentTaskName.ToUpper())"
	Write-Output "##teamcity[progessMessage '$message']"
}

task compile { 
	exec { msbuild $solution_file /m /property:"Configuration=$build_configuration;OutputPath=$build_output_dir" /nologo }
}
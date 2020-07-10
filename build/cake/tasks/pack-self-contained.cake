Task("Pack-Self-Contained")
    .IsDependentOn("Generate-Docs")
    .Does<BuildParameters>((context,parameters) =>  {

       PackPrepareNative(context, parameters);

});    
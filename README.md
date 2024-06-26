# Alex Notes

To build this project: SHIFT-CMD-P, Tasks: Run Task, build(functions)

# General Notes

This project was cloned for use in the Selfie.Live backend.
Build with dotnet core 3.1.420 installed (errors when trying to use v6)

Ref: https://rainstormtech.com/dynamic-image-resizing-with-azure-functions-storage-and-cdn

I updated the image processing library from v1.x to v2.1.3

```
dotnet add package SixLabors.ImageSharp
```

I added a call to AutoOrient() in the Image.Mutate() call, per this thread: https://stackoverflow.com/questions/68518051/sixlabors-imagesharp-crop-resizes-wrong-width-and-height

To get C# intellisense working I had to move my C# VSC extension back to the previous version, v1.24.4
<br>
<br>

# Running locally

rename example json to local.settings.json, set AzureWebJobsStorage (get from Azure function app env var)  
SHIFT-CMD-P, Tasks: Run Task, func: host start  
The function will run at this url: http://localhost:7071/api/ResizeImage  
Note that the endpoint is case insensitive, you can call xxx/resizeimage  
In Postman call POST on the local endpoint, must pass in ResizeImagePayload as the body; see younique-server, image.service.ts resize()

# Deployment

Go to the Azure VSC extension, open Function App, right click on image-resizer-cs3 and choose "Deploy to Function App..."  
<br>

# ImageResizeProxy

This project is a demo that shows how to use this Azure Function (3.1) to dynamically resize images that are held in an Azure Storage container.

There are several query string switches that can be used. Here are all of them currently

```
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?size=small
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?size=medium
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?size=hero
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?w=200
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?h=300
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?w=400&h=300
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?output=png
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?output=gif
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?mode=stretch
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?mode=boxpad
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?mode=pad
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?mode=max
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?mode=min
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?mode=crop
// all together
https://function-endpoint.azurewebsites.net/api/resizeimage/someimage.jpg?w=400&h=300&output=png&mode=stretch
```

To run locally, rename the example.local.settings.json file to local.settings.json and update the settings to fit your environment.

To run in Azure, simply publish the project to a new Azure Function and create the following Application Settings in the Azure Portal:

1. AzureContainer - name of the container
2. ImageResizer:HeroSize - 1440x620
3. ImageResizer:MediumSize - 400x400
4. ImageResizer:SmallSize - 200x200
5. ClientCache:MaxAge - 30.00:00:00

And add in a Connection String called "AzureStorage" that cooresponds with your storage account.

Happy Resizing.

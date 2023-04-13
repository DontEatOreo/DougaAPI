## DougaAPI

DougaAPI is a specialized API designed exclusively for the DougaBot.

Each JSON structure inherits from ModelBase thus each end point needs to have the following properties as a base:
```json
{
  "url": "string",
  "max_file_size": "int"
}
```
*Note: The max file size is provided by the client since a server could be boosted and have higher upload limit thus avoiding uploading to a 3rd party host*

## Expected JSON structures:

## Compression

**Endpoints:** 
````
compression/video
compression/audio
````

**JSON:**
```json
{
  "ios_compatible": "bool",
  "resolution": "string?",
  "crf": "int",
  "bitrate": "int?"
}
```
Note: Bitrate in this case is for the audio only. 

## Speed

**Endpoint:**
```
speed
```

**JSON:**
```json
{
  "speed": "double"
}
```

## Trim

**Endpoint:**
```
trim
```

**JSON:**
```json
{
  "start": "float",
  "end": "float"
}
```

## Error Codes
Should the client provide an invalid URL or an error occur on the API side, the API will return an error code in the headers along with a JSON structure containing the error code and message:

```json
{
  "status": "int",
  "message": "string"
}
```
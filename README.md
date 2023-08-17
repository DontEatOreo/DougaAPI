# DougaAPI

DougaAPI is a specialized API designed exclusively for the DougaBot.

Before running this project make sure you have `ffmpeg` and `yt-dlp`.

## appsettings.json

You will need to edit the `Settings` section of the `appsettings.json` file to match your environment.

Here is an example:

```json
{
  "Settings": {
    "YtdlpPath": "yt-dlp",
    "FfmpegPath": "ffmpeg",
    "MaxDuration": "02:00:00",
    "MaxTrimTime": "00:15:00",
    "MaxCompressTime": "00:05:00"
  }
}
```

## Notes

- `YtdlpPath` and `FfmpegPath` are the paths to the executables, in this example we assume they are in the `PATH` environment variable.
- `MaxDuration`, is the maximum duration of a video no matter the operation (download, compress, trim).
- `MaxTrimTime` is the maximum duration of a video when trimming.
- `MaxCompressTime` is the maximum duration of a video when compressing.
- So in your example case a video can be at most 2 hours long, the trimming time is 15 minutes and the compressing time is 5 minutes.

## JSON Structure

Each JSON structure inherits from QueryBase thus each end point needs to have the following properties as a base:

For example if you want to just download a video you will need to make the following requests:

```bash
curl --location 'https://<dougaapi-url>/compress' \
--header 'Content-Type: application/json' \
--data '{
    "uri": "<video-link>"
}'
```

## Endpoints

```text
download
compress
trim
toaudio
```

## Errors

The API will return an error code in the headers along with a JSON structure containing the error message:

```json
{
  "error": "string"
}
```

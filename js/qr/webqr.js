// QRCODE reader Copyright 2011 Lazar Laszlo
// http://www.webqr.com

var gCtx = null;
var gCanvas = null;
var c=0;
var video;

function initCanvas(ww,hh)
{
    gCanvas = document.getElementById("qr-canvas");
    var w = ww;
    var h = hh;
    gCanvas.style.width = w + "px";
    gCanvas.style.height = h + "px";
    gCanvas.width = w;
    gCanvas.height = h;
    gCtx = gCanvas.getContext("2d");
    gCtx.clearRect(0, 0, w, h);
}

function captureToCanvas() {
    
	try{
		gCtx.drawImage(video, 0, 0);
		try{
			qrcode.decode();
		}
		catch(e){       
			console.log(e);
			setTimeout(captureToCanvas, 500);
		};
	}
	catch(e){       
		console.log(e);
		setTimeout(captureToCanvas, 500);
	};
    
}

function htmlEntities(str) {
    return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function read(a)
{
    var html="<br>";
    if(a.indexOf("http://") === 0 || a.indexOf("https://") === 0)
        html+="<a target='_blank' href='"+a+"'>"+a+"</a><br>";
    html+="<b>"+htmlEntities(a)+"</b><br><br>";
    document.getElementById("result").innerHTML = html;
    console.log(a);
}	

function isCanvasSupported(){
	var elem = document.createElement('canvas');
	return !!(elem.getContext && elem.getContext('2d'));
}

function mediaSuccess(stream) {
	video = document.getElementById("scanVideoFrame");
	video.src = window.URL.createObjectURL(stream);
	video.play();
	
	setTimeout(captureToCanvas, 500);
}
		
function mediaError(err) {
	console.log("[ERR] could not get video stream");
	console.log(err);
}

function load()
{
	if(isCanvasSupported())
	{
		initCanvas(800, 600);
		qrcode.callback = read;
		navigator.getMedia = (navigator.webkitGetUserMedia || navigator.mozGetUserMedia);		
		navigator.getMedia({video: true, audio: false}, mediaSuccess, mediaError);    
	}
	else
	{
		console.log("[ERR] No canvas supported");
	}
}








function QRScanner(videoTagName, cDetected)
{	
	var m_video = null;
	var m_canvas = null;
	var m_gctx = null;
	var m_scan_active = false;
	var m_imageData = null;
	var callbackDetected = cDetected;
	var that = this;
	
	this.mediaSuccess = function(stream)
	{
		m_video.src = window.URL.createObjectURL(stream);
		m_video.play();
	};
	
	this.startScan = function()
	{
		setTimeout(this.captureToCanvas, 500);
		$(m_video).show();
		m_scan_active = true;
	};
	
	this.stopScan = function()
	{
		$(m_video).hide();
		m_scan_active = false;
	};
	
	this.captureToCanvas = function()
	{
		try{
			m_gctx.drawImage(m_video, 0, 0);
			try{
				qrcode.decode();
			}
			catch(e){
				console.log(e);
				if(m_scan_active)
					setTimeout(function(thisObj){ thisObj.captureToCanvas(); }, 500, that);
			};
		}
		catch(e){
			console.log(e);
			if(m_scan_active)
				setTimeout(function(thisObj){ thisObj.captureToCanvas(); }, 500, that);
		};
		
	};
	
	this.isCanvasSupported = function()
	{
		var elem = document.createElement('canvas');
		return !!(elem.getContext && elem.getContext('2d'));
	};
	
	this.qrDetected = function(data)
	{
		that.stopScan();
		
		var qrReg = /(BS([0-9]{2})([0-9]{4}))([a-zA-Z0-9]{40})/g
		qrReg.exec(data);
		if(RegExp.$1 && RegExp.$2 && RegExp.$3 && RegExp.$4)
		{
			if(CryptoJS.SHA1(RegExp.$1) == RegExp.$4)
			{
				var cType = parseInt(RegExp.$2);
				var cID = parseInt(RegExp.$3);
				
				console.log("Card Valid");
				console.log("Card type: " + cType);
				console.log("Card ID: " + cID);
				callbackDetected(cType, cID);
			}else
			{
				console.log("Card Invalid!");
			}
		}
		// (BS[0-9]{6})([a-zA-Z0-9]{40})
		
		console.log(data);
		
	};
	
	m_video = document.getElementById(videoTagName);
	
	if(this.isCanvasSupported())
	{
		var cwidth = 640;
		var cheight = 480;
		
		//m_canvas = document.createElement("canvas");
		//m_canvas.id = "qr-canvas";
		m_canvas = document.getElementById("qr-canvas");
		m_canvas.style.display = "none";
		m_canvas.style.width = cwidth + "px";
		m_canvas.style.height = cheight + "px";
		m_canvas.width = cwidth;
		m_canvas.height = cheight;
		m_gctx = m_canvas.getContext("2d");
		m_gctx.clearRect(0, 0, cwidth, cheight);
		m_imageData = m_gctx.getImageData(0,0,320,240);
		
		
		qrcode.callback = this.qrDetected;
		navigator.getMedia = (navigator.webkitGetUserMedia || navigator.mozGetUserMedia);		
		navigator.getMedia({video: true, audio: false}, this.mediaSuccess,
														function(err)
														{
															console.log("[ERR] could not get video stream");
															console.log(err);
														});
		$(m_video).hide();
	}
	else
	{
		console.log("[ERR] No canvas supported");
	}
	
	
};


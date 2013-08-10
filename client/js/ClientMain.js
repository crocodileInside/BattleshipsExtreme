var config = {
	field_x: 10,
	field_y: 10	
};

var connection;

var messageStack = new Array();
var dequeue = false;

/* Players */
var players = new Array();

function Player()
{
	this.name = "";
	this.field = new Array(new Array(config.field_x), new Array(config.field_y));
}

function onPush ()
{
	if (!dequeue)
	{
		dequeue = true;
		
		while (messageStack.length)
		{
			var message = messageStack.shift();
			
			handleMessage(message);
		}
		
		dequeue = false;
	}
}


function handleMessage (message)
{
	var messageObj = JSON.parse(message);
	
	if (messageObj != null)
	{
		switch (messageObj.origin)
		{
			case "server":
				switch (messageObj.action)
				{
					case "allowMessages":
						stateMachine.sendHandle("messagesAllowed");
						break;
				}
				break;
		}
	}
}


function sendMessage (action, data)
{
	var message = new Object();
	message.action = action;
	if (data != null)
	{
		message.data = data;
	}
	
	connection.send(JSON.stringify(message));
}


$(document).ready(function(){
	var stateMachine = new StateMachine();

	// Event bindings
	$('#connect').click(function() {
		stateMachine.sendHandle('connect');
	});
});


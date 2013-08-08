var cardTypes = [ {cardName: "Schuss", cardCooldown: 1, cardImage: "schuss.png", cardDescription: "Feuert einen Schuss auf ein Feld ab."}, 
{cardName: "Artillerieangriff", cardCooldown: 1, cardImage: "artillerie.png", cardDescription: "Zerstört ein komplettes Schiff (wenn ungepanzert)"}, 
{cardName: "Clusterbombe", cardCooldown: 1, cardImage: "cluster.png", cardDescription: "Feuert einen Schuss auf 5 Felder ab (in Plus-Formation)"}, 
{cardName: "Blindgänger", cardCooldown: 2, cardImage: "blind.png", cardDescription: "Der nächste Schuss des Gegners trifft nicht."}, 
{cardName: "Schild", cardCooldown: 2, cardImage: "schild.png", cardDescription: "Schützt ein beliebiges Feld vor einem Schuss."}, 
{cardName: "Schiff neu bauen", cardCooldown: 2, cardImage: "rebuild.png", cardDescription: "Baut ein zerstörtes Schiff komplett wieder auf"}, 
{cardName: "Schiff reparieren", cardCooldown: 2, cardImage: "repair.png", cardDescription: "Repariert ein beschädigtes Schiff komplett"}, 
{cardName: "Spion befragen", cardCooldown: 2, cardImage: "spion_fragen.png", cardDescription: "Der Spion liefert euch eine nützliche Position."}, 
{cardName: "Spion zurückholen", cardCooldown: 2, cardImage: "spion_holen.png", cardDescription: "Trefft euch mit dem Spion und befragt ihn ausführlich."}, 
 ];


String.prototype.lpad = function(padString, length) {
	var str = this;
	while (str.length < length)
		str = padString + str;
	return str;
}



var cardDB = new Array();
var cardIndexCounter = 0;

function generateCardQRString(cardType, cardID)
{
	var strQR = "BS" + cardType.toString().lpad("0", 2) + cardID.toString().lpad("0", 4);
	strQR = strQR + CryptoJS.SHA1(strQR).toString();
	return strQR;
}

function generateCardQRURL(data)
{
	return "https://chart.googleapis.com/chart?cht=qr&chld=M&chs=400x400&chl="+data;
}

function addCardsToDB(cType, num)
{
	for(var i = 0; i < num; i++)
	{
		cardDB.push({cardID: cardIndexCounter, cardType: cType, cardQR: generateCardQRString(cType, cardIndexCounter)});
		cardIndexCounter++;
	}
}

function generateCardDB()
{
	cardIndexCounter = 0;
	
	addCardsToDB(0, 50);	//Schuss
	addCardsToDB(1, 20);	//Artillerie
	addCardsToDB(2, 20);	//Cluster
	addCardsToDB(3, 20);	//Blindgänger
	addCardsToDB(4, 10);	//Schild
	addCardsToDB(5, 10);	//neubauen
	addCardsToDB(6, 10);	//reparieren
	addCardsToDB(7, 10);	//SP befragen
	addCardsToDB(8, 10);	//SP zurückholen
	
	console.log(cardDB);
}



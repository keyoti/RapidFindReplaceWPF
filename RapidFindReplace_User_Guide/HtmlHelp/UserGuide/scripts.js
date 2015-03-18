function toggleExpander(id){
	var element = null;
	var imageElement = null;
	if (document.all) {
		element = document.all[''+id+'Content'];
		imageElement = document.all[''+id+''];
	} else if (document.getElementById) {
		element = document.getElementById(''+id+'Content');
		imageElement = document.getElementById(''+id+'');
	}
	if(element == null) return
	else if (document.all) (element.style.display == "block")?element.style.display = "none":element.style.display = "block";
	else if (document.getElementById) (element.style.display == "block")?element.style.display = "none":element.style.display = "block";

	if(imageElement.src.indexOf("/Images/expander-open.png")>-1)
		imageElement.src = imageElement.src.replace("/Images/expander-open.png", "/Images/expander-closed.png");
	else
		imageElement.src = imageElement.src.replace("/Images/expander-closed.png", "/Images/expander-open.png");
}





function show(lang){
	document.cookie = "codeChoice="+lang;
	var els = document.getElementsByTagName("code");
	
	for(var e=0; e<els.length; e++){
		if(els[e].className==lang || lang=='all')
			els[e].style.display = "block";
		else
			els[e].style.display = "none";
	}
}

function readCookie(name) {
	var nameEQ = name + "=";
	var ca = document.cookie.split(';');
	for(var i=0;i < ca.length;i++) {
		var c = ca[i];
		while (c.charAt(0)==' ') c = c.substring(1,c.length);
		if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);
	}
	return null;
}

function chooseLanguage(){
	var codeChoice = readCookie('codeChoice');
	
	if(codeChoice !='cs' && codeChoice !='vb')
		codeChoice ="all";
	show(codeChoice);

	var chooser = document.getElementById('choice'+codeChoice);
	if(chooser)
		chooser.checked=true;
}

window.onload = function(){
	chooseLanguage();
}
				

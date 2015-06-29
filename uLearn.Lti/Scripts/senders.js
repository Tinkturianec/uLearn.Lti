function submitScore() {
	var score = $('#score').val();
	$.ajax({
		type: 'GET',
		url: '@Url.Action("Score")',
		data: { score: score }
	})
		.success(sendScoreToConsumer)
		.fail(function(res) { showError(res); });
}

function sendScoreToConsumer() {
	$.ajax({
		type: 'GET',
		url: '@Url.Action("SubmitScore")',
	})
		.fail(function(res) { showError(res); });
}

function showError(response) {
	$('#error').text = response.responseText;
}
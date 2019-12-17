
local isView2d = false;

local chart;
local highway;

local fakeAudio;
local audio;

local quantizeSteps = { 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 192 };
local currentQuantizeIndex = 8;

local function measureDuration() return chart.mostRecentControlPointAtTime(audio.position).measureDuration; end

local function getQuantizeStep() return quantizeSteps[currentQuantizeIndex]; end
local function quantizeAudio()
	local step = measureDuration() / getQuantizeStep();
	audio.position = math.floor(0.5 + audio.position / step) * step;
end

function theori.layer.doAsyncLoad()
	chart = msc.charts.loadXmlFile("testchart.txt");
	--chart = msc.charts.createNew();

	highway = msc.graphics.createHighway(chart);
	if (not highway.asyncLoad()) then
		return false;
	end

	return true;
end

function theori.layer.doAsyncFinalize()
	if (not highway.asyncFinalize()) then
		return false;
	end

	highway.lookAhead = 3;

	fakeAudio = theori.audio.createFakeAudio(41000, 2);

	return true;
end

function theori.layer.init()
	audio = fakeAudio;
	audio.position = chart.timeStart;

	theori.input.keyboard.pressed:connect(function(key)
		if (key == KeyCode.TAB) then
			isView2d = not isView2d;
		end

		if (isView2d) then
		else
			if (key == KeyCode.D) then
				highway.lanesHaveDepth = not highway.lanesHaveDepth;
			end
		end

		if (key == KeyCode.SPACE) then
			if (audio.isPlaying) then
				audio.stop();
				quantizeAudio();
			else
				audio.play();
			end
		end

		--if (not audio.isPlaying) then
			if (key == KeyCode.UP) then
				audio.position = audio.position + measureDuration() / getQuantizeStep();
			elseif (key == KeyCode.DOWN) then
				audio.position = audio.position - measureDuration() / getQuantizeStep();
			elseif (key == KeyCode.PAGEUP) then
				audio.position = audio.position + measureDuration();
			elseif (key == KeyCode.PAGEDOWN) then
				audio.position = audio.position - measureDuration();
			end
		--end
	end);
end

function theori.layer.destroy()
end

function theori.layer.suspended()
end

function theori.layer.resumed()
end

function theori.layer.update(delta, total)
	timer = total;

	if (isView2d) then
	else
		local width, height = theori.graphics.getViewportSize();
		if (width > height) then
			highway.setViewport((width - height * 0.7) * 0.5, height * 0.2, height * 0.7);
		else
			highway.setViewport(0, (height - width) - width * 0.1, width);
		end

		highway.position = audio.position;
		highway.update();
	end
end

function theori.layer.render()
	if (isView2d) then
	else
		highway.render();
	end
end

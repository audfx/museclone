
local TOOL_NORMAL = 1;
local TOOL_SMALL_SPIN = 2;
local TOOL_LARGE_SPIN = 3;

local currentEntity = { nil, nil, nil, nil, nil, nil };

local isView2d = false;

local chart;
local highway;

local tool = TOOL_NORMAL;

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

local function createEntityForLane(lane)
	if (tool == TOOL_NORMAL) then
		return theori.charts.newEntity("Museclone.Button");
	elseif (lane < 5 and (tool == TOOL_SMALL_SPIN or tool == LARGE_SPIN)) then
		return theori.charts.newEntity("Museclone.Spinner");
	end

	return nil;
end

local function laneKeyPressed(lane)
	if (currentEntity[lane]) then return; end

	isPlacingEntity = true;
	print("pressed lane " .. tostring(lane));
	-- assumes the time is currently quantized
	local time = audio.position;
	local tick = chart.calcTickFromTime(time);

	local entity = chart.getEntityAtTick(lane, tick, true);
	if (entity) then
		print("  remove entity");
		chart.removeEntity(entity);
	else
		local newEntity = createEntityForLane(lane);
		print("  create entity:" .. tostring(newEntity));
		if (newEntity) then
			newEntity.position = tick;

			chart.addEntity(lane, newEntity);
			currentEntity[lane] = newEntity;
		end
	end

	highway.refresh();
end

local function laneKeyReleased(lane)
	if (not currentEntity[lane]) then return; end

	currentEntity[lane] = nil;
end

local function adjustEntityDurations()
	local tick = chart.calcTickFromTime(audio.position);
	for lane, entity in next, currentEntity do
		entity.duration = math.max(0, tick - entity.position);
		print(entity.duration);
	end
	highway.refresh();
end

local function releaseEdits()
	for lane, entity in next, currentEntity do
		currentEntity[lane] = nil;
	end
end

function theori.layer.doAsyncLoad()
	--chart = msc.charts.loadXmlFile("testchart.txt");
	chart = msc.charts.create();

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

		if (not audio.isPlaying) then
			if (key == KeyCode.D1) then
				laneKeyPressed(0);
			elseif (key == KeyCode.D2) then
				laneKeyPressed(1);
			elseif (key == KeyCode.D3) then
				laneKeyPressed(2);
			elseif (key == KeyCode.D4) then
				laneKeyPressed(3);
			elseif (key == KeyCode.D5) then
				laneKeyPressed(4);
			elseif (key == KeyCode.BACKQUOTE) then
				laneKeyPressed(5);
			end
		end

		if (key == KeyCode.SPACE) then
			releaseEdits();
			if (audio.isPlaying) then
				audio.stop();
				quantizeAudio();
			else
				audio.play();
			end
		elseif (key == KeyCode.UP) then
			audio.position = audio.position + measureDuration() / getQuantizeStep();
			adjustEntityDurations();
		elseif (key == KeyCode.DOWN) then
			audio.position = audio.position - measureDuration() / getQuantizeStep();
			adjustEntityDurations();
		elseif (key == KeyCode.PAGEUP) then
			audio.position = audio.position + measureDuration();
			adjustEntityDurations();
		elseif (key == KeyCode.PAGEDOWN) then
			audio.position = audio.position - measureDuration();
			adjustEntityDurations();
		elseif (key == KeyCode.END) then
			audio.position = chart.TimeStart;
			adjustEntityDurations();
		elseif (key == KeyCode.HOME) then
			audio.position = chart.TimeEnd;
			adjustEntityDurations();
		end
	end);

	theori.input.keyboard.released:connect(function(key)
		if (not audio.isPlaying) then
			if (key == KeyCode.D1) then
				laneKeyReleased(0);
			elseif (key == KeyCode.D2) then
				laneKeyReleased(1);
			elseif (key == KeyCode.D3) then
				laneKeyReleased(2);
			elseif (key == KeyCode.D4) then
				laneKeyReleased(3);
			elseif (key == KeyCode.D5) then
				laneKeyReleased(4);
			elseif (key == KeyCode.BACKQUOTE) then
				laneKeyReleased(5);
			end
		end
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

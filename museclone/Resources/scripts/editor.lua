
local TOOL_NORMAL = 1;
local TOOL_SMALL_SPIN = 2;
local TOOL_LARGE_SPIN = 3;

local currentEntity = { nil, nil, nil, nil, nil, nil };
local currentEditManner = nil;

local isView2d = false;

local chart, highway;
local fakeAudio, audio;

local tool = TOOL_NORMAL;

-- 2D Column Stuff

local column2dCameraPosition = 0;
local quarterNotesPerColumn = 4 * 4;

local menuHeight = 16; -- File, Edit, etc.
local toolbarHeight = 24; -- Editor tools

local columnMargin, columnPadding = 16, 32;
local columnLaneWidth = 11;
local columnWidth = 5 * columnLaneWidth;

local editSpaceWidth, editSpaceHeight;
local editSpaceX, editSpaceY;

local maxVisibleColumns;

local mouseHovered2dColumnLane = -1;
local mouseHovered2dColumnX, mouseHovered2dColumnY = 0, 0;
local mouseHovered2dColumn = 0;
local mouseIsIn2dColumn = false;

-- ------------------

local quantizeSteps = { 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 192 };
local currentQuantizeIndex = 8;

local function measureDuration() return chart.mostRecentControlPointAtTime(audio.position).measureDuration; end

local function getQuantizeStep() return quantizeSteps[currentQuantizeIndex]; end

local function quantizeTimeFloor(time)
	local step = measureDuration() / getQuantizeStep();
	return math.floor(time / step) * step;
end

local function quantizeTimeRound(time)
	local step = measureDuration() / getQuantizeStep();
	return math.floor(0.5 + time / step) * step;
end

local function quantizeAudio()
	audio.position = quantizeTimeRound(audio.position);
	return audio.position;
end

local function getColumnTickRange(columnIndex)
	-- todo(local): have this actually calculate where columns are
	return columnIndex * 4, 4;
end

local function getColumnFromTick(position)
	-- todo(local): have this actually find the correct column
	return math.floor(position / 4);
end

local function getPositionAlongRange(startTick, tickDuration, position)
	local startTime, endTime = chart.calcTimeFromTick(startTick), chart.calcTimeFromTick(startTick + tickDuration);
	local positionTime = chart.calcTimeFromTick(position);
	
	return (positionTime - startTime) / (endTime - startTime);
end

local function createEntityForLane(lane)
	if (tool == TOOL_NORMAL) then
		return theori.charts.newEntity("Museclone.Button");
	elseif (lane < 5 and (tool == TOOL_SMALL_SPIN or tool == LARGE_SPIN)) then
		return theori.charts.newEntity("Museclone.Spinner");
	end

	return nil;
end

local function laneMousePressed(button, lane, tick)
	local thisManner = "mouse" .. tostring(button);
	if (currentEntity[lane] or currentEditManner) then return; end
	
	if (tick < 0) then return; end

	print("clicked lane " .. tostring(lane));

	local entity = chart.getEntityAtTick(lane, tick, true);
	if (entity) then
		if (button == MouseButton.Left) then
			print("  select entity");
			
			currentEditManner = thisManner;
			currentEntity[lane] = entity;
		elseif (button == MouseButton.Right) then
			print("  remove entity");
			chart.removeEntity(entity);
		end
	else
		if (button == MouseButton.Left) then
			local newEntity = createEntityForLane(lane);
			print("  create entity:" .. tostring(newEntity));
			if (newEntity) then
				currentEditManner = thisManner;

				newEntity.position = tick;

				chart.addEntity(lane, newEntity);
				currentEntity[lane] = newEntity;
			end
		end
	end

	highway.refresh();
end

local function laneMouseReleased(button, lane, tick)
	local thisManner = "mouse" .. tostring(button);
	if (not currentEntity[lane] or currentEditManner ~= thisManner) then return; end

	currentEntity[lane] = nil;
	currentEditManner = nil;
end

local function laneKeyPressed(lane)
	if (currentEntity[lane] or currentEditManner) then return; end
	
	-- assumes the time is currently quantized
	local time = audio.position;
	local tick = chart.calcTickFromTime(time);
	
	if (tick < 0) then return; end

	print("pressed lane " .. tostring(lane));

	local entity = chart.getEntityAtTick(lane, tick, true);
	if (entity) then
		print("  remove entity");
		chart.removeEntity(entity);
	else
		local newEntity = createEntityForLane(lane);
		print("  create entity:" .. tostring(newEntity));
		if (newEntity) then
			currentEditManner = "keyboard";

			newEntity.position = tick;

			chart.addEntity(lane, newEntity);
			currentEntity[lane] = newEntity;
		end
	end

	highway.refresh();
end

local function laneKeyReleased(lane)
	if (not currentEntity[lane] or currentEditManner != "keyboard") then return; end

	currentEntity[lane] = nil;
	currentEditManner = nil;
end

local function adjustEntityDurations(locationTick)
	local tick = locationTick or chart.calcTickFromTime(audio.position);
	for lane, entity in next, currentEntity do
		entity.duration = math.max(0, tick - entity.position);
	end
	highway.refresh();
end

local function releaseEdits()
	for lane, entity in next, currentEntity do
		currentEntity[lane] = nil;
	end
end

-- returns (isInColumn, columnIndex, columnRelativeX, columnRelativeY)
local function getMousePositionIn2dEditSpace()
	local w, h = theori.graphics.getViewportSize();
	local x, y = theori.input.mouse.getMousePosition();
	
	if (y > editSpaceY and y < editSpaceY + editSpaceHeight) then
		local columnSpace = (x - columnMargin + columnPadding * 0.5 + column2dCameraPosition - w / 2) / (columnWidth + columnPadding);

		local columnIndex = math.floor(columnSpace);
		local columnRelativeX = math.max(0, math.min(1, (-columnPadding * 0.5 + (columnWidth + columnPadding) * (columnSpace - columnIndex)) / columnWidth));
		local columnRelativeY = math.max(0, math.min(1, 1 - (y - editSpaceY) / editSpaceHeight));

		return true, columnIndex, columnRelativeX, columnRelativeY;
	end

	return false, 0, 0, 0;
end

function theori.layer.doAsyncLoad()
	chart = msc.charts.loadXmlFile("testchart.txt");
	--chart = msc.charts.create();

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

	theori.input.mouse.pressed:connect(function(button, x, y)
		if (isView2d) then
			if (mouseIsIn2dColumn) then
				laneMousePressed(button, mouseHovered2dColumnLane, mouseHovered2dTickPosition);
			end
		end
	end);

	theori.input.mouse.released:connect(function(button, x, y)
		if (isView2d) then
			if (mouseIsIn2dColumn) then
				laneMouseReleased(button, mouseHovered2dColumnLane, mouseHovered2dTickPosition);
			end
		end
	end);

	theori.input.mouse.moved:connect(function(x, y)
		if (currentEditManner and string.sub(currentEditManner, 1, 5) == "mouse") then
			adjustEntityDurations(mouseHovered2dTickPosition);
		end
	end);

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
	
	local width, height = theori.graphics.getViewportSize();

	if (isView2d) then
		local audioPositionTicks = chart.calcTickFromTime(audio.position);

		local currentColumn = getColumnFromTick(audioPositionTicks);
		local columnStart, columnLength = getColumnTickRange(currentColumn);
		local columnProgress = getPositionAlongRange(columnStart, columnLength, audioPositionTicks);
		
		editSpaceWidth, editSpaceHeight = width - 2 * columnMargin, height - menuHeight - toolbarHeight - 2 * columnMargin; -- The screen space for columns
		editSpaceX, editSpaceY = 0, menuHeight + toolbarHeight + columnMargin;
		
		maxVisibleColumns = math.ceil(editSpaceWidth / (columnWidth + columnPadding)) + 1;

		column2dCameraPosition = (columnWidth + columnPadding) * (currentColumn + columnProgress);
		column2dCameraPosition = math.max(column2dCameraPosition, width / 2);

		mouseIsIn2dColumn, mouseHovered2dColumn, mouseHovered2dColumnX, mouseHovered2dColumnY = getMousePositionIn2dEditSpace();
		mouseHovered2dColumnLane = math.min(4, math.floor(mouseHovered2dColumnX * 5));

		local hoveredColumnStart, hoveredColumnLength = getColumnTickRange(mouseHovered2dColumn);
		local mouseYTime = quantizeTimeRound(chart.calcTimeFromTick(hoveredColumnStart) + mouseHovered2dColumnY * (chart.calcTimeFromTick(hoveredColumnStart + hoveredColumnLength) - chart.calcTimeFromTick(hoveredColumnStart)));
		mouseHovered2dTickPosition = chart.calcTickFromTime(mouseYTime);
	else
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
	local width, height = theori.graphics.getViewportSize();

	if (isView2d) then
		local audioPositionTicks = chart.calcTickFromTime(audio.position);

		local xOffset = column2dCameraPosition - width / 2;

		local columnOffset = math.floor(xOffset / (columnWidth + columnPadding));

		for i = columnOffset, maxVisibleColumns - 1 + columnOffset do
			local columnPositionX = columnMargin + editSpaceX - xOffset + i * (columnWidth + columnPadding);
			local columnStart, columnLength = getColumnTickRange(i);

			theori.graphics.setFillToColor(180, 0, 0, 255);
			chart.forEachEntityInRangeTicks(5, columnStart, columnStart + columnLength, function(entity)
				if (entity.duration == 0) then
					return;
				end

				local entityColumnOffset = getPositionAlongRange(columnStart, columnLength, entity.position);
				local yPos = editSpaceY + editSpaceHeight * (1 - entityColumnOffset);
				
				local yPosTop;
				if (entity.endPosition >= columnStart + columnLength) then
					yPosTop = editSpaceY;
				else
					local entityColumnLength = getPositionAlongRange(columnStart, columnLength, entity.endPosition);
					yPosTop = editSpaceY + editSpaceHeight * (1 - entityColumnLength);
				end
						
				local holdWidth = columnLaneWidth * 3;
				theori.graphics.fillRect(columnPositionX + columnLaneWidth, yPosTop, holdWidth, yPos - yPosTop);
			end);

			theori.graphics.setFillToColor(70, 70, 70, 255);
			for c = 0, 4 do
				theori.graphics.fillRect(columnPositionX + math.floor(columnLaneWidth * (c + 0.5)), editSpaceY, 1, editSpaceHeight);
			end

			if (audioPositionTicks >= columnStart and audioPositionTicks <= columnStart + columnLength) then
				local cursorColumnOffset = getPositionAlongRange(columnStart, columnLength, audioPositionTicks);

				theori.graphics.setFillToColor(255, 0, 0, 255);
				theori.graphics.fillRect(columnPositionX, editSpaceY + (editSpaceHeight - 1) * (1 - cursorColumnOffset), columnWidth, 1);
			end

			for l = 5, 0, -1 do
				if (l == 5) then
					theori.graphics.setFillToColor(255, 255, 255, 255);
				elseif (l % 2 == 0) then
					theori.graphics.setFillToColor(127, 225, 255, 255);
				else
					theori.graphics.setFillToColor(255, 245, 150, 255);
				end

				chart.forEachEntityInRangeTicks(l, columnStart, columnStart + columnLength, function(entity)
					if (l == 5 and entity.duration > 0) then
						return;
					end
					
					local entityColumnOffset = getPositionAlongRange(columnStart, columnLength, entity.position);
					local yPos = editSpaceY + editSpaceHeight * (1 - entityColumnOffset);

					if (entity.duration > 0) then
						local yPosTop;
						if (entity.endPosition >= columnStart + columnLength) then
							yPosTop = editSpaceY;
						else
							local entityColumnLength = getPositionAlongRange(columnStart, columnLength, entity.endPosition);
							yPosTop = editSpaceY + editSpaceHeight * (1 - entityColumnLength);
						end
						
						local holdWidth = columnLaneWidth - 6;
						theori.graphics.fillRect(columnPositionX + math.floor(columnLaneWidth * (l + 0.5)) - holdWidth / 2, yPosTop, holdWidth, yPos - yPosTop);
					else
						if (l == 5) then
							local instantHeight = math.floor(columnLaneWidth / 2);
							theori.graphics.fillRect(columnPositionX + columnLaneWidth, yPos - instantHeight, columnLaneWidth * 3, instantHeight);
						else
							local instantSize = columnLaneWidth - 4;
							theori.graphics.fillRect(columnPositionX + math.floor(columnLaneWidth * (l + 0.5)) - instantSize / 2, yPos - instantSize / 2, instantSize, instantSize);
						end
					end
				end);
			end

			if (mouseIsIn2dColumn and i == mouseHovered2dColumn) then
				local hoveredLanePosition = columnPositionX + columnLaneWidth * mouseHovered2dColumnLane;
				local hoveredLaneTickPosition = getPositionAlongRange(columnStart, columnLength, mouseHovered2dTickPosition);

				--theori.graphics.setFillToColor(255, 255, 0, 45);
				--theori.graphics.fillRect(hoveredLanePosition, editSpaceY, columnLaneWidth, editSpaceHeight);
			
				theori.graphics.setFillToColor(0, 180, 0, 255);
				theori.graphics.fillRect(columnPositionX, editSpaceY + (editSpaceHeight - 1) * (1 - hoveredLaneTickPosition), columnWidth, 1);
			end
		end
	else
		highway.render();
	end
end

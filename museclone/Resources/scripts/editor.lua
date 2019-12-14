
local chart;
local highway;

local timer = -6;

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

	return true;
end

function theori.layer.init()
	theori.input.keyboard.pressed:connect(function(key)
		if (key == KeyCode.D) then
			highway.lanesHaveDepth = not highway.lanesHaveDepth;
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

    if (width > height) then
        highway.setViewport((width - height * 0.95) * 0.5, 0, height * 0.95);
    else
		highway.setViewport(0, (height - width) * 0.5 - width * 0.2, width);
	end

	highway.position = timer;
	highway.update();
end

function theori.layer.render()
	highway.render();
end

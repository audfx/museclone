
local chart;

function theori.layer.doAsyncLoad()
	return true;
end

function theori.layer.init()
	local chart = msc.charts.loadXmlFile("testchart.txt");

	for cp in chart.controlPoints do
	end

	for lane in chart.lanes do
		print(tostring(lane.label));

		for entity in lane.entities do
			print("    " .. entity.typeId .. ", " .. tostring(entity.position) .. ", " .. tostring(entity.duration));
		end
	end
end

function theori.layer.destroy()
end

function theori.layer.suspended()
end

function theori.layer.resumed()
end

function theori.layer.update(delta, total)
end

function theori.layer.render()
end

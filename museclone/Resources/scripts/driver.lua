
function theori.layer.doAsyncLoad()
	return theori.doStaticLoadsAsync();
end

function theori.layer.doAsyncFinalize()
	return theori.finalizeStaticLoads();
end

function theori.layer.init()
    theori.charts.setDatabaseToClean(function()
        theori.charts.setDatabaseToPopulate(function() print("Populate (from driver) finished."); end);
    end);

    theori.layer.setInvalidForResume();
end
